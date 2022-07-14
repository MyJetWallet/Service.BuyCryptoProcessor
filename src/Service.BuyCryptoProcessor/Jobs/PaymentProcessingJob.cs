using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Circle.Models.Payments;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using Newtonsoft.Json;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.BuyCryptoProcessor.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models.Enums;
using Service.BuyCryptoProcessor.Postgres;
using Service.ChangeBalanceGateway.Grpc;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Converter.Grpc;
using Service.Liquidity.Converter.Grpc.Models;

namespace Service.BuyCryptoProcessor.Jobs
{
    public class PaymentProcessingJob : IStartable, IDisposable
    {
        private readonly MyTaskTimer _timer;
        private readonly ILogger<PaymentProcessingJob> _logger;
        private readonly IServiceBusPublisher<CryptoBuyIntention> _publisher;
        private readonly ICircleDepositService _circleDepositService;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;
        private readonly IQuoteService _quoteService;
        private readonly ISpotChangeBalanceService _changeBalanceService;

        public PaymentProcessingJob(ILogger<PaymentProcessingJob> logger, IServiceBusPublisher<CryptoBuyIntention> publisher, ISubscriber<Deposit> depositSubscriber, ICircleDepositService circleDepositService, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, IQuoteService quoteService, ISpotChangeBalanceService changeBalanceService)
        {
            _logger = logger;
            _publisher = publisher;
            _circleDepositService = circleDepositService;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            _quoteService = quoteService;
            _changeBalanceService = changeBalanceService;
            depositSubscriber.Subscribe(HandleCircleDeposits);
            _timer = new MyTaskTimer(typeof(PaymentProcessingJob),
                TimeSpan.FromSeconds(Program.Settings.TimerPeriodInSec),
                logger, DoTime);
        }

        private async Task DoTime()
        {
            await GetCheckoutUrl();
            await ExecuteQuote();
            await TransferFundsToClient();
        }
        
        private async Task GetCheckoutUrl()
        {
            using var activity = MyTelemetry.StartActivity("Handle update approved payments");
            try
            {
                await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

                var sw = new Stopwatch();
                sw.Start();

                var count = 0;
                var intentions = await context.Intentions.Where(e =>
                    e.Status == BuyStatus.ExecutionStarted).ToListAsync();

                var updatedIntentions = new List<CryptoBuyIntention>();
                foreach (var intention in intentions)
                    try
                    {
                        var response = await _circleDepositService.GetCirclePayment(new ()
                        {
                            BrokerId = intention.BrokerId,
                            DepositId = intention.CircleDepositId
                        });

                        if (response.Success && response.Data.Status is PaymentStatus.ActionRequired or PaymentStatus.Confirmed)
                        {
                            intention.Status = BuyStatus.PaymentCreated;
                            intention.DepositCheckoutLink = response.Data.RequiredAction?.RedirectUrl;
                            await PublishSuccess(intention);
                            updatedIntentions.Add(intention);
                            count++;
                            continue;
                        }

                        if (response.Data.Status is PaymentStatus.Failed)
                        {
                            intention.Status = BuyStatus.Failed;
                            intention.PaymentExecutionErrorCode = response.Data.ErrorCode;
                            await PublishSuccess(intention);
                            updatedIntentions.Add(intention);
                            count++;
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        await HandleError(intention, ex);
                    }

                if(updatedIntentions.Any())
                    await context.UpsertAsync(updatedIntentions);

                intentions.Count.AddToActivityAsTag("payment-count");

                sw.Stop();
                if (count > 0)
                    _logger.LogInformation("Handled {countTrade} approved payments. Time: {timeRangeText}",
                        count,
                        sw.Elapsed.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Handle approved payments");
                ex.FailActivity();
                throw;
            }

        }

        private async ValueTask HandleCircleDeposits(Deposit deposit)
        {
            if (deposit.BeneficiaryClientId != Program.Settings.ServiceClientId)
                return;
            
            if (deposit.Status != DepositStatus.Processed && deposit.Status != DepositStatus.Error)
                return;
            
            var intentionId = deposit.CryptoBuyData?.CryptoBuyId;
            if(string.IsNullOrEmpty(intentionId))
                return;

            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var intention = await context.Intentions.FirstOrDefaultAsync(t => t.Id == intentionId);
            if (intention == null)
                return;
            
            try
            {
                if (deposit.Status == DepositStatus.Error)
                {
                    intention.Status = BuyStatus.Failed;
                    intention.PaymentExecutionErrorCode = ConvertErrorCode(deposit.PaymentProviderErrorCode);
                }
                else
                {
                    intention.Status = BuyStatus.PaymentReceived;
                    intention.ProvidedCryptoAsset = deposit.AssetSymbol;
                    intention.ProvidedCryptoAmount = deposit.Amount;
                }
                intention.DepositOperationId = deposit.Id.ToString();
                intention.DepositTimestamp = deposit.EventDate;
                intention.DepositIntegration = deposit.Integration;
                intention.CardLast4 = deposit.CardLast4;
                await PublishSuccess(intention);
            }
            catch (Exception ex)
            {
                await HandleError(intention, ex);
            }

            await context.UpsertAsync(new[] {intention});
        }
        
        private async Task ExecuteQuote()
        {
            using var activity = MyTelemetry.StartActivity("Handle payment to execute quote");
            try
            {
                await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

                var sw = new Stopwatch();
                sw.Start();

                var count = 0;
                var intentions = await context.Intentions.Where(e =>
                    e.Status == BuyStatus.PaymentReceived).ToListAsync();

                var updatedIntentions = new List<CryptoBuyIntention>();
                foreach (var intention in intentions)
                    try
                    {
                        if (intention.ProvidedCryptoAsset == intention.BuyAsset)
                        {
                            intention.Status = BuyStatus.ConversionExecuted;
                            await PublishSuccess(intention);
                            updatedIntentions.Add(intention); 
                            count++;
                            continue;
                        }
                        var quoteResponse = await _quoteService.ExecuteQuoteAsync(new ExecuteQuoteRequest
                        {
                            FromAsset = intention.ProvidedCryptoAsset,
                            ToAsset = intention.BuyAsset,
                            FromAssetVolume = intention.ProvidedCryptoAmount,
                            ToAssetVolume = intention.BuyAmount,
                            IsFromFixed = true,
                            BrokerId = intention.BrokerId,
                            AccountId = Program.Settings.ServiceClientId,
                            WalletId = Program.Settings.ServiceWalletId,
                            OperationId = intention.PreviewQuoteId,
                            Price = intention.QuotePrice
                        });
                       
                        if(quoteResponse.QuoteExecutionResult == QuoteExecutionResult.Success)
                        {
                            UpdateIntentionAfterSwap(intention, quoteResponse);
                            await PublishSuccess(intention);
                            updatedIntentions.Add(intention); 
                            count++;
                            continue;
                        }

                        if (quoteResponse.QuoteExecutionResult == QuoteExecutionResult.ReQuote)
                        {
                            var reQuoteResponse = await _quoteService.ExecuteQuoteAsync(new ExecuteQuoteRequest
                            {
                                FromAsset = intention.ProvidedCryptoAsset,
                                ToAsset = intention.BuyAsset,
                                FromAssetVolume = intention.ProvidedCryptoAmount,
                                ToAssetVolume = quoteResponse.Data.FromAssetVolume,
                                IsFromFixed = true,
                                BrokerId = intention.BrokerId,
                                AccountId = Program.Settings.ServiceClientId,
                                WalletId = Program.Settings.ServiceWalletId,
                                OperationId = quoteResponse.Data.OperationId,
                                Price = quoteResponse.Data.Price
                            });

                            if (reQuoteResponse.QuoteExecutionResult == QuoteExecutionResult.Success)
                            {
                                UpdateIntentionAfterSwap(intention, quoteResponse);
                                await PublishSuccess(intention);
                                updatedIntentions.Add(intention); 
                                count++;
                                continue;
                            }
                        }
                        
                        var quote = await _quoteService.GetQuoteAsync(new GetQuoteRequest
                        {
                            FromAsset = intention.ProvidedCryptoAsset,
                            ToAsset = intention.BuyAsset,
                            FromAssetVolume = intention.ProvidedCryptoAmount,
                            IsFromFixed = true,
                            BrokerId = intention.BrokerId,
                            AccountId = Program.Settings.ServiceClientId,
                            WalletId = Program.Settings.ServiceWalletId,
                            QuoteType = QuoteType.CryptoBuy,
                            ProfileId = intention.SwapProfileId,
                            CryptoBuyId = intention.Id
                        });
                        
                        quoteResponse = await _quoteService.ExecuteQuoteAsync(new ExecuteQuoteRequest
                        {
                            FromAsset = intention.ProvidedCryptoAsset,
                            ToAsset = intention.BuyAsset,
                            FromAssetVolume = intention.ProvidedCryptoAmount,
                            ToAssetVolume = quote.Data.ToAssetVolume,
                            IsFromFixed = true,
                            BrokerId = intention.BrokerId,
                            AccountId = Program.Settings.ServiceClientId,
                            WalletId = Program.Settings.ServiceWalletId,
                            OperationId = quote.Data.OperationId,
                            Price = quote.Data.Price,
                        });
                        
                        if(quoteResponse.QuoteExecutionResult == QuoteExecutionResult.Success)
                        {
                            UpdateIntentionAfterSwap(intention, quoteResponse);
                            await PublishSuccess(intention);
                            updatedIntentions.Add(intention); 
                            count++;
                        }
                        else
                        {
                            throw new Exception(quote.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        await HandleError(intention, ex);
                    }

                if(updatedIntentions.Any())
                    await context.UpsertAsync(updatedIntentions);

                intentions.Count.AddToActivityAsTag("payment-count");

                sw.Stop();
                if (count > 0)
                    _logger.LogInformation("Handled {countTrade} payments to execute quote. Time: {timeRangeText}",
                        count,
                        sw.Elapsed.ToString());
                
                //locals

                void UpdateIntentionAfterSwap(CryptoBuyIntention intention, QuoteExecutionResponse quoteResponse)
                {
                    intention.BuyAmount = quoteResponse.Data.ToAssetVolume;
                    intention.SwapFeeAmount = quoteResponse.Data.FeeAmount;
                    intention.SwapFeeAsset = quoteResponse.Data.FeeAsset;
                    intention.ExecuteQuoteId = quoteResponse.Data.OperationId;
                    intention.ExecuteTimestamp = DateTime.UtcNow;
                    intention.Status = BuyStatus.ConversionExecuted;
                    intention.QuotePrice = quoteResponse.Data.Price;
                    
                    intention.SwapFeeAmountConverted = Math.Round(intention.SwapFeeAmount / intention.QuotePrice, 2);;
                    
                    intention.Rate = Math.Round(1/quoteResponse.Data.Price, 2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Handle payment to execute quote");
                ex.FailActivity();
                throw;
            }
        }

        private async Task TransferFundsToClient()
        {
            using var activity = MyTelemetry.StartActivity("Handle payment transfers to client");
            try
            {
                await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

                var sw = new Stopwatch();
                sw.Start();

                var count = 0;
                var intentions = await context.Intentions.Where(e =>
                    e.Status == BuyStatus.ConversionExecuted).ToListAsync();

                var updatedIntentions = new List<CryptoBuyIntention>();
                foreach (var intention in intentions)
                    try
                    {
                        var response = await _changeBalanceService.TransferCryptoBuyFundsAsync(new () 
                        {
                            TransactionId = intention.Id,
                            CryptoBuyClientId = Program.Settings.ServiceClientId,
                            CryptoBuyWalletId = Program.Settings.ServiceWalletId,
                            ClientWalletId = intention.WalletId,
                            Amount = intention.BuyAmount,
                            AssetSymbol = intention.BuyAsset,
                            BrokerId = intention.BrokerId
                        });

                        if (response.Result)
                        {
                            intention.Status = BuyStatus.Finished;
                            await PublishSuccess(intention);
                            updatedIntentions.Add(intention);
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        await HandleError(intention, ex);
                    }

                if(updatedIntentions.Any())
                    await context.UpsertAsync(updatedIntentions);

                intentions.Count.AddToActivityAsTag("transfers-count");

                sw.Stop();
                if (count > 0)
                    _logger.LogInformation("Handled {countTrade} payment transfers to client. Time: {timeRangeText}",
                        count,
                        sw.Elapsed.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Handle payment transfers to client");
                ex.FailActivity();
                throw;
            }
            
        }


        private async Task HandleError(CryptoBuyIntention intention, Exception ex, bool retrying = true)
        {
            ex.FailActivity();
            
            intention.WorkflowState =  WorkflowState.Retrying;
            
            intention.LastError = ex.Message.Length > 2048 ? ex.Message.Substring(0, 2048) : ex.Message;
            intention.RetriesCount++;
            if (intention.RetriesCount >= Program.Settings.RetriesLimit || !retrying)
            {
                intention.WorkflowState = WorkflowState.Failed;
            }

            _logger.LogError(ex,
                "CryptoBuy ID {operationId} changed workflow state to {status}. Operation: {operationJson}",
                intention.Id, intention.WorkflowState, JsonConvert.SerializeObject(intention));
            try
            {
                await _publisher.PublishAsync(intention);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Can not publish error operation status {operationJson}",
                    JsonConvert.SerializeObject(intention));
            }
        }

        private async Task PublishSuccess(CryptoBuyIntention intention)
        {
            var retriesCount = intention.RetriesCount;
            var lastError = intention.LastError;
            var state = intention.WorkflowState;
            try
            {
                intention.RetriesCount = 0;
                intention.LastError = null;
                intention.WorkflowState = WorkflowState.Normal;

                await _publisher.PublishAsync(intention);
                _logger.LogInformation(
                    "CryptoBuy with Operation ID {operationId} is changed to status {status}. Operation: {operationJson}",
                    intention.Id, intention.Status, JsonConvert.SerializeObject(intention));
            }
            catch
            {
                intention.RetriesCount = retriesCount;
                intention.LastError = lastError;
                intention.WorkflowState = state;
                throw;
            }
        }
        
        public void Start()
        {
            _timer.Start();
        }
        

        public void Dispose()
        {
            _timer?.Dispose();
        }
        
        private static PaymentErrorCode? ConvertErrorCode(PaymentProviderErrorCode depositPaymentProviderErrorCode)
        {
            return depositPaymentProviderErrorCode switch
            {
                PaymentProviderErrorCode.OK => null,
                PaymentProviderErrorCode.PaymentFailed => PaymentErrorCode.PaymentFailed,
                PaymentProviderErrorCode.PaymentFraudDetected => PaymentErrorCode.PaymentFraudDetected,
                PaymentProviderErrorCode.PaymentDenied => PaymentErrorCode.PaymentDenied,
                PaymentProviderErrorCode.PaymentNotSupportedByIssuer => PaymentErrorCode.PaymentNotSupportedByIssuer,
                PaymentProviderErrorCode.PaymentNotFunded => PaymentErrorCode.PaymentNotFunded,
                PaymentProviderErrorCode.PaymentUnprocessable => PaymentErrorCode.PaymentUnprocessable,
                PaymentProviderErrorCode.PaymentStoppedByIssuer => PaymentErrorCode.PaymentStoppedByIssuer,
                PaymentProviderErrorCode.PaymentCanceled => PaymentErrorCode.PaymentCanceled,
                PaymentProviderErrorCode.PaymentReturned => PaymentErrorCode.PaymentReturned,
                PaymentProviderErrorCode.PaymentFailedBalanceCheck => PaymentErrorCode.PaymentFailedBalanceCheck,
                PaymentProviderErrorCode.CardFailed => PaymentErrorCode.CardFailed,
                PaymentProviderErrorCode.CardInvalid => PaymentErrorCode.CardInvalid,
                PaymentProviderErrorCode.CardAddressMismatch => PaymentErrorCode.CardAddressMismatch,
                PaymentProviderErrorCode.CardZipMismatch => PaymentErrorCode.CardZipMismatch,
                PaymentProviderErrorCode.CardCvvInvalid => PaymentErrorCode.CardCvvInvalid,
                PaymentProviderErrorCode.CardExpired => PaymentErrorCode.CardExpired,
                PaymentProviderErrorCode.CardLimitViolated => PaymentErrorCode.CardLimitViolated,
                PaymentProviderErrorCode.CardNotHonored => PaymentErrorCode.CardNotHonored,
                PaymentProviderErrorCode.CardCvvRequired => PaymentErrorCode.CardCvvRequired,
                PaymentProviderErrorCode.CreditCardNotAllowed => PaymentErrorCode.CreditCardNotAllowed,
                PaymentProviderErrorCode.CardAccountIneligible => PaymentErrorCode.CardAccountIneligible,
                PaymentProviderErrorCode.CardNetworkUnsupported => PaymentErrorCode.CardNetworkUnsupported,
                PaymentProviderErrorCode.ChannelInvalid => PaymentErrorCode.ChannelInvalid,
                PaymentProviderErrorCode.UnauthorizedTransaction => PaymentErrorCode.UnauthorizedTransaction,
                PaymentProviderErrorCode.BankAccountIneligible => PaymentErrorCode.BankAccountIneligible,
                PaymentProviderErrorCode.BankTransactionError => PaymentErrorCode.BankTransactionError,
                PaymentProviderErrorCode.InvalidAccountNumber => PaymentErrorCode.InvalidAccountNumber,
                PaymentProviderErrorCode.InvalidWireRtn => PaymentErrorCode.InvalidWireRtn,
                PaymentProviderErrorCode.InvalidAchRtn => PaymentErrorCode.InvalidAchRtn,
                PaymentProviderErrorCode.RefIdInvalid => PaymentErrorCode.RefIdInvalid,
                PaymentProviderErrorCode.AccountNameMismatch => PaymentErrorCode.AccountNameMismatch,
                PaymentProviderErrorCode.AccountNumberMismatch => PaymentErrorCode.AccountNumberMismatch,
                PaymentProviderErrorCode.AccountIneligible => PaymentErrorCode.AccountIneligible,
                PaymentProviderErrorCode.WalletAddressMismatch => PaymentErrorCode.WalletAddressMismatch,
                PaymentProviderErrorCode.CustomerNameMismatch => PaymentErrorCode.CustomerNameMismatch,
                PaymentProviderErrorCode.InstitutionNameMismatch => PaymentErrorCode.InstitutionNameMismatch,
                PaymentProviderErrorCode.VerificationFailed => PaymentErrorCode.VerificationFailed,
                PaymentProviderErrorCode.VerificationFraudDetected => PaymentErrorCode.VerificationFraudDetected,
                PaymentProviderErrorCode.VerificationDenied => PaymentErrorCode.VerificationDenied,
                PaymentProviderErrorCode.VerificationNotSupportedByIssuer => PaymentErrorCode.VerificationNotSupportedByIssuer,
                PaymentProviderErrorCode.VerificationStoppedByIssuer => PaymentErrorCode.VerificationStoppedByIssuer,
                PaymentProviderErrorCode.ThreeDSecureNotSupported => PaymentErrorCode.ThreeDSecureNotSupported,
                PaymentProviderErrorCode.ThreeDSecureRequired => PaymentErrorCode.ThreeDSecureRequired,
                PaymentProviderErrorCode.ThreeDSecureFailure => PaymentErrorCode.ThreeDSecureFailure,
                PaymentProviderErrorCode.ThreeDSecureActionExpired => PaymentErrorCode.ThreeDSecureActionExpired,
                PaymentProviderErrorCode.ThreeDSecureInvalidRequest => PaymentErrorCode.ThreeDSecureInvalidRequest,
                PaymentProviderErrorCode.CardRestricted => PaymentErrorCode.CardRestricted,
                _ => throw new ArgumentOutOfRangeException(nameof(depositPaymentProviderErrorCode),
                    depositPaymentProviderErrorCode, null)
            };

        }

    }
}