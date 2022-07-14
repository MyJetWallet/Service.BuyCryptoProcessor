using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.BuyCryptoProcessor.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models.Enums;
using Service.BuyCryptoProcessor.Grpc;
using Service.BuyCryptoProcessor.Grpc.Models;
using Service.BuyCryptoProcessor.Postgres;
using Service.Fees.Client;
using Service.Fees.Domain.Models;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Converter.Grpc;
using Service.Liquidity.Converter.Grpc.Models;
using CryptoBuyData = Service.BuyCryptoProcessor.Grpc.Models.CryptoBuyData;

namespace Service.BuyCryptoProcessor.Services
{
    public class CryptoBuyService: ICryptoBuyService
    {
        private readonly ILogger<CryptoBuyService> _logger;
        private readonly IDepositFeesClient _depositFees;
        private readonly IQuoteService _quoteService;
        private readonly ICircleDepositService _circleService;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;
        private readonly IIndexPricesClient _indexPricesClient;
        
        public CryptoBuyService(ILogger<CryptoBuyService> logger, IDepositFeesClient depositFees, IQuoteService quoteService, ICircleDepositService circleService, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, IIndexPricesClient indexPricesClient)
        {
            _logger = logger;
            _depositFees = depositFees;
            _quoteService = quoteService;
            _circleService = circleService;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            _indexPricesClient = indexPricesClient;
        }

        public async Task<CreateCryptoBuyResponse> CreateCryptoBuy(CreateCryptoBuyRequest request)
        {
            try
            {
                var intentionId = Guid.NewGuid().ToString("N");
                var intention = new CryptoBuyIntention
                {
                    Id = intentionId,
                    ClientId = request.ClientId,
                    BrandId = request.BrandId,
                    BrokerId = request.BrokerId,
                    WalletId = request.WalletId,
                    CreationTime = DateTime.UtcNow,
                    Status = BuyStatus.New,
                    WorkflowState = WorkflowState.Normal,
                    ServiceWalletId = Program.Settings.ServiceWalletId,
                    DepositProfileId = request.DepositFeeProfileId,
                    PaymentMethod = request.PaymentMethod,
                    PaymentAmount = request.PaymentAmount,
                    PaymentAsset = request.PaymentAsset,
                    SwapProfileId = request.SwapFeeProfileId,
                    BuyAsset = request.BuyAsset
                };

                var providedCryptoAsset = GetCryptoAsset(request.BuyAsset, request.PaymentMethod);
                var buyFees =
                    _depositFees.GetDepositFees(request.BrokerId, request.DepositFeeProfileId, providedCryptoAsset);

                intention.BuyFeeAsset = intention.PaymentAsset;
                intention.BuyFeeAmount = buyFees.FeeSizeType switch
                {
                    FeeSizeType.Absolute => buyFees.FeeSizeAbsolute,
                    FeeSizeType.Percentage => intention.PaymentAmount * buyFees.FeeSizeRelative  / 100m,
                    FeeSizeType.Composite => (intention.PaymentAmount * buyFees.FeeSizeRelative  / 100m) +
                                             buyFees.FeeSizeAbsolute
                };

                var providedCryptoAmount = intention.PaymentAmount - intention.BuyFeeAmount;
                intention.ProvidedCryptoAmount = providedCryptoAmount;
                intention.ProvidedCryptoAsset = providedCryptoAsset;

                if (intention.ProvidedCryptoAsset != intention.BuyAsset)
                {
                    var quote = await _quoteService.GetQuoteAsync(new GetQuoteRequest
                    {
                        FromAsset = intention.ProvidedCryptoAsset,
                        ToAsset = request.BuyAsset,
                        FromAssetVolume = intention.ProvidedCryptoAmount,
                        IsFromFixed = true,
                        BrokerId = request.BrokerId,
                        AccountId = Program.Settings.ServiceClientId,
                        WalletId = Program.Settings.ServiceWalletId,
                        QuoteType = QuoteType.CryptoBuy,
                        ProfileId = request.SwapFeeProfileId,
                        CryptoBuyId = intentionId
                    });

                    if (!quote.IsSuccess)
                    {
                        return new CreateCryptoBuyResponse
                        {
                            IsSuccess = false,
                            ConverterResponse = quote.ErrorCode,
                            ErrorCode = CryptoBuyErrorCode.ConverterError
                        };
                    }

                    intention.PreviewQuoteId = quote.Data.OperationId;
                    intention.BuyAmount = quote.Data.ToAssetVolume;
                    intention.SwapFeeAmount = quote.Data.FeeAmount;
                    intention.SwapFeeAsset = quote.Data.FeeAsset;
                    intention.PreviewConvertTimestamp = DateTime.UtcNow;
                    intention.QuotePrice = quote.Data.Price;
                    
                    Console.WriteLine(quote.Data.ToJson());

                    intention.SwapFeeAmountConverted = Math.Round(intention.SwapFeeAmount / intention.QuotePrice, 2);
                    intention.SwapFeeAssetConverted = intention.BuyAsset;
                    intention.Rate = Math.Round(1/quote.Data.Price, 2);
                }
                else
                {
                    intention.BuyAmount = providedCryptoAmount;
                    intention.SwapFeeAmount = 0m;
                    intention.SwapFeeAsset = providedCryptoAsset;
                    
                    intention.SwapFeeAmountConverted = Math.Round(intention.SwapFeeAmount / intention.QuotePrice, 2);
                    intention.SwapFeeAssetConverted = intention.BuyAsset;

                    intention.Rate = 1;
                }
                
                intention.BuyAssetIndexPrice =
                    _indexPricesClient.GetIndexPriceByAssetAsync(intention.BuyAsset).UsdPrice;
                intention.PaymentAssetIndexPrice =
                    _indexPricesClient.GetIndexPriceByAssetAsync(intention.PaymentAsset).UsdPrice;
                intention.BuyFeeAssetIndexPrice =
                    _indexPricesClient.GetIndexPriceByAssetAsync(intention.BuyFeeAsset).UsdPrice;
                intention.SwapFeeAssetIndexPrice =
                    _indexPricesClient.GetIndexPriceByAssetAsync(intention.SwapFeeAsset).UsdPrice;
                
                await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
                await context.UpsertAsync(new[] {intention});

                return new CreateCryptoBuyResponse
                {
                    IsSuccess = true,
                    Data = new CryptoBuyData()
                    {
                        PaymentMethod = intention.PaymentMethod,
                        PaymentAmount = intention.PaymentAmount,
                        PaymentAsset = intention.PaymentAsset,
                        BuyAmount = intention.BuyAmount,
                        BuyAsset = intention.BuyAsset,
                        DepositFeeAmount = intention.BuyFeeAmount,
                        DepositFeeAsset = intention.BuyFeeAsset,
                        TradeFeeAmount = intention.SwapFeeAmount,
                        TradeFeeAsset = intention.SwapFeeAsset,
                        //TradeFeeAmount = intention.SwapFeeAmountConverted,
                        //TradeFeeAsset = intention.SwapFeeAssetConverted,
                        Rate = intention.Rate,
                        PaymentId = intention.Id
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When creating payment for request {request}", request.ToJson());
                return new CreateCryptoBuyResponse()
                {
                    IsSuccess = false,
                    ErrorCode = CryptoBuyErrorCode.InternalServerError
                };
            }
        }

        public async Task<ExecuteCryptoBuyResponse> ExecuteCryptoBuy(ExecuteCryptoBuyRequest request)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var intention = await context.Intentions.FirstOrDefaultAsync(t => t.Id == request.PaymentId);
            if (intention == null)
            {
                return new ExecuteCryptoBuyResponse()
                {
                    IsSuccess = false,
                    ErrorCode = CryptoBuyErrorCode.OperationNotFound
                };
            }

            if (intention.Status != BuyStatus.New)
            {
                var isSuccess = intention.PaymentCreationErrorCode is null or AddCardDepositResponse.StatusCode.Ok
                                && intention.PaymentExecutionErrorCode is null;
                return new ExecuteCryptoBuyResponse()
                {
                    IsSuccess = isSuccess,
                    PaymentCreationErrorCode = intention.PaymentCreationErrorCode,
                    PaymentExecutionErrorCode = intention.PaymentExecutionErrorCode,
                    ErrorCode = CryptoBuyErrorCode.CircleError
                };
            }

            if(string.IsNullOrEmpty(intention.CircleRequestId))
                intention.CircleRequestId = Guid.NewGuid().ToString("D");

            var response = await _circleService.AddCirclePayment(new AddCardDepositRequest
            {
                BrokerId = intention.BrokerId,
                ClientId = intention.ClientId,
                WalletId = intention.WalletId,
                RequestId = intention.CircleRequestId,
                KeyId = request.CirclePaymentDetails.KeyId,
                SessionId = request.CirclePaymentDetails.SessionId,
                IpAddress = request.CirclePaymentDetails.IpAddress,
                Amount = intention.PaymentAmount,
                Currency = intention.ProvidedCryptoAsset,
                CardId = request.CirclePaymentDetails.CardId,
                EncryptedData = request.CirclePaymentDetails.EncryptedData,
                CryptoBuyId = intention.Id,
                CryptoBuyClientId = Program.Settings.ServiceClientId,
                CryptoBuyWalletId = Program.Settings.ServiceWalletId
            });

            intention.CircleDepositId = response.DepositId;
            intention.CardId = request.CirclePaymentDetails.CardId;

            if (response.Status == AddCardDepositResponse.StatusCode.Ok)
                intention.Status = BuyStatus.ExecutionStarted;
            else
            {
                intention.PaymentCreationErrorCode = response.Status;
                intention.LastError = response.Status.ToString();
                intention.Status = BuyStatus.Failed;
            }

            await context.UpsertAsync(new[] {intention});
            
            return new ExecuteCryptoBuyResponse()
            {
                IsSuccess = response.Status == AddCardDepositResponse.StatusCode.Ok,
                PaymentCreationErrorCode = response.Status,
                ErrorCode = CryptoBuyErrorCode.CircleError
            };
        }

        public async Task<CryptoBuyStatusResponse> GetCryptoBuyStatus(CryptoBuyStatusRequest request)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var intention = await context.Intentions.FirstOrDefaultAsync(t => t.Id == request.PaymentId);
            if (intention == null)
            {
                return new CryptoBuyStatusResponse
                {
                    PaymentId = null
                };
            }

            var response = new CryptoBuyStatusResponse()
            {
                PaymentId = intention.Id,
                Status = intention.Status,
                PaymentInfo = new Payment
                {
                    PaymentAsset = intention.PaymentAsset,
                    PaymentAmount = intention.PaymentAmount,
                    DepositFeeAmount = intention.BuyFeeAmount,
                    DepositFeeAsset = intention.BuyFeeAsset,
                    TradeFeeAmount = intention.SwapFeeAmount,
                    TradeFeeAsset = intention.SwapFeeAsset,
                    //TradeFeeAmount = intention.SwapFeeAmountConverted,
                    //TradeFeeAsset = intention.SwapFeeAssetConverted,
                }
            };

            if (intention.Status is BuyStatus.PaymentCreated && intention.PaymentMethod == PaymentMethods.CircleCard)
                response.ClientAction = new ClientAction()
                {
                    CheckoutUrl = intention.DepositCheckoutLink,
                    RedirectUrls = new List<string>()
                        {Program.Settings.CircleSuccessUrl, Program.Settings.CircleFailureUrl}
                };

            if (intention.Status is BuyStatus.ConversionExecuted or BuyStatus.Finished)
                response.BuyInfo = new Buy
                {
                    BuyAmount = intention.BuyAmount,
                    BuyAsset = intention.BuyAsset,
                    Rate = intention.Rate
                };

            if (intention.Status is BuyStatus.Failed)
            {
                response.PaymentExecutionErrorCode = intention.PaymentExecutionErrorCode;
                response.PaymentCreationErrorCode = intention.PaymentCreationErrorCode;
            }

            return response;
        }


        private static string GetCryptoAsset(string paymentAsset, PaymentMethods paymentMethod)
        {
            //TODO: get assets dictionary
            return "USDC";
        }
    }
    
}
