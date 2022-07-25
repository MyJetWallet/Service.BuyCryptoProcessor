using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Unlimint.Settings.Services;
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
    public class CryptoBuyService : ICryptoBuyService
    {
        private readonly ILogger<CryptoBuyService> _logger;
        private readonly IDepositFeesClient _depositFees;
        private readonly IQuoteService _quoteService;
        private readonly ICircleDepositService _circleService;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;
        private readonly IIndexPricesClient _indexPricesClient;
        private readonly IUnlimintDepositService _unlimintDepositService;
        private readonly IUnlimintAssetMapper _unlimintAssetMapper;

        public CryptoBuyService(ILogger<CryptoBuyService> logger, IDepositFeesClient depositFees, IQuoteService quoteService, ICircleDepositService circleService, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, IIndexPricesClient indexPricesClient, IUnlimintDepositService unlimintDepositService,
            IUnlimintAssetMapper unlimintAssetMapper)
        {
            _logger = logger;
            _depositFees = depositFees;
            _quoteService = quoteService;
            _circleService = circleService;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            _indexPricesClient = indexPricesClient;
            _unlimintDepositService = unlimintDepositService;
            _unlimintAssetMapper = unlimintAssetMapper;
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

                var providedCryptoAsset = GetCryptoAsset(request.PaymentAsset, request.PaymentMethod);

                var buyFeeSize = GetBuyFeeAmount(
                    request.BrokerId, 
                    request.DepositFeeProfileId, 
                    providedCryptoAsset,
                    request.PaymentMethod, 
                    request.PaymentAmount);

                intention.BuyFeeAsset = providedCryptoAsset;
                intention.BuyFeeAmount = buyFeeSize;
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

                    intention.SwapFeeAmountConverted = Math.Round(intention.SwapFeeAmount / intention.QuotePrice, 2);
                    intention.SwapFeeAssetConverted = intention.BuyAsset;
                    intention.Rate = Math.Round(1 / quote.Data.Price, 2);
                }
                else
                {
                    intention.BuyAmount = providedCryptoAmount;
                    intention.SwapFeeAmount = 0m;
                    intention.SwapFeeAsset = providedCryptoAsset;

                    intention.SwapFeeAmountConverted = 0m;
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
                await context.UpsertAsync(new[] { intention });

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
            _logger.LogInformation("Requested execution of intention {intentionId}", request.PaymentId);
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var intention = await context.Intentions.FirstOrDefaultAsync(t => t.Id == request.PaymentId);
            if (intention == null)
            {
                _logger.LogWarning("Requested intention does not exists {intentionId}", request.PaymentId);
                return new ExecuteCryptoBuyResponse()
                {
                    IsSuccess = false,
                    ErrorCode = CryptoBuyErrorCode.OperationNotFound
                };
            }

            var isSuccess = false;
            var circleCode = AddCardDepositResponse.StatusCode.Ok;

            if (intention.Status != BuyStatus.New)
            {
                isSuccess = intention.PaymentCreationErrorCode is null or AddCardDepositResponse.StatusCode.Ok
                            && intention.PaymentErrorCode is PaymentProviderErrorCode.OK;
                return new ExecuteCryptoBuyResponse()
                {
                    IsSuccess = isSuccess,
                    PaymentCreationErrorCode = intention.PaymentCreationErrorCode,
                    PaymentErrorCode = intention.PaymentErrorCode,
                    ErrorCode = CryptoBuyErrorCode.CircleError
                };
            }

            if (request.PaymentMethod == PaymentMethods.CircleCard)
            {
                var circleResponse = await RequestCirclePayment(intention, request.CirclePaymentDetails);
                isSuccess = circleResponse.Status == AddCardDepositResponse.StatusCode.Ok;
                circleCode = circleResponse.Status;
            }

            if (request.PaymentMethod == PaymentMethods.Unlimint)
            {
                var unlimintResponse = await RequestUnlimintPayment(intention, request.UnlimintPaymentDetails);
                isSuccess = unlimintResponse.Status == AddCardDepositResponse.StatusCode.Ok;
                circleCode = unlimintResponse.Status;
            }

            await context.UpsertAsync(new[] { intention });

            return new ExecuteCryptoBuyResponse()
            {
                IsSuccess = isSuccess,
                PaymentCreationErrorCode = circleCode,
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

            if (intention.Status is BuyStatus.PaymentCreated)
                response.ClientAction = new ClientAction()
                {
                    CheckoutUrl = intention.DepositCheckoutLink,
                    RedirectUrls = new List<string>()
                        {
                        Program.Settings.CircleSuccessUrl,
                        Program.Settings.CircleFailureUrl,
                        Program.Settings.Cancel3dUrl,
                        Program.Settings.InProcess3dUrl,
                        Program.Settings.Return3dUrl,}
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
                response.PaymentErrorCode = intention.PaymentErrorCode;
                response.PaymentCreationErrorCode = intention.PaymentCreationErrorCode;
            }

            return response;
        }


        private string GetCryptoAsset(string paymentAsset, PaymentMethods paymentMethod)
        {
            //TODO: get assets dictionary
            if (paymentMethod == PaymentMethods.CircleCard)
                return "USDC";
            if (paymentMethod == PaymentMethods.Unlimint)
            {
                var asset = _unlimintAssetMapper.GetUnlimintByPaymentAsset("jetwallet", paymentAsset);
                if (asset != null)
                    return asset.SettlementAsset;
            }

            throw new Exception("Unsupported asset");
        }

        private decimal GetBuyFeeAmount(string brokerId, string feeProfile, string providedCryptoAsset, PaymentMethods method, decimal paymentAmount)
        {
            var network = method switch
            {
                PaymentMethods.CircleCard => "circle-card",
                PaymentMethods.Unlimint => "unlimint-card",
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };
            var buyFees =
                _depositFees.GetDepositFees(brokerId, feeProfile, providedCryptoAsset, network);

            var buyFeeSize = buyFees.FeeSizeType switch
            {
                FeeSizeType.Absolute => buyFees.FeeSizeAbsolute,
                FeeSizeType.Percentage => paymentAmount * buyFees.FeeSizeRelative / 100m,
                FeeSizeType.Composite => (paymentAmount * buyFees.FeeSizeRelative / 100m) +
                                         buyFees.FeeSizeAbsolute
            };

            return buyFeeSize;
        }

        private async Task<AddCardDepositResponse> RequestCirclePayment(CryptoBuyIntention intention,
            CirclePaymentDetails paymentDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(intention.PaymentProcessorRequestId))
                    intention.PaymentProcessorRequestId = Guid.NewGuid().ToString("D");

                var response = await _circleService.AddCirclePayment(new AddCardDepositRequest
                {
                    BrokerId = intention.BrokerId,
                    ClientId = intention.ClientId,
                    WalletId = intention.WalletId,
                    RequestId = intention.PaymentProcessorRequestId,
                    KeyId = paymentDetails.KeyId,
                    SessionId = paymentDetails.SessionId,
                    IpAddress = paymentDetails.IpAddress,
                    Amount = intention.PaymentAmount,
                    Currency = intention.ProvidedCryptoAsset,
                    CardId = paymentDetails.CardId,
                    EncryptedData = paymentDetails.EncryptedData,
                    CryptoBuyId = intention.Id,
                    CryptoBuyClientId = Program.Settings.ServiceClientId,
                    CryptoBuyWalletId = Program.Settings.ServiceWalletId
                });

                intention.CircleDepositId = response.DepositId;
                intention.CardId = paymentDetails.CardId;

                if (response.Status == AddCardDepositResponse.StatusCode.Ok)
                {
                    intention.Status = BuyStatus.ExecutionStarted;
                    _logger.LogInformation(
                        "Receiver successful response for intentionId {intentionId} from circle {response}",
                        intention.Id, response);
                }
                else
                {
                    _logger.LogError(
                        "Receiver unsuccessful response for intentionId {intentionId} from circle {response}",
                        intention.Id, response);
                    intention.PaymentCreationErrorCode = response.Status;
                    intention.LastError = response.Status.ToString();
                    intention.Status = BuyStatus.Failed;
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When requesting circle payment for circle {intentionId}", intention.Id);
                throw;
            }
        }

        private async Task<UnlimintCardPaymentResponse> RequestUnlimintPayment(CryptoBuyIntention intention, UnlimintPaymentDetails paymentDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(intention.PaymentProcessorRequestId))
                    intention.PaymentProcessorRequestId = Guid.NewGuid().ToString("D");

                var response = await _unlimintDepositService.AddUnlimintPayment(new UnlimintCardPaymentRequest
                {
                    BrokerId = intention.BrokerId,
                    ClientId = intention.ClientId,
                    WalletId = intention.WalletId,
                    MerchantOrderId = intention.PaymentProcessorRequestId,
                    IpAddress = paymentDetails?.IpAddress ?? String.Empty,
                    Amount = intention.PaymentAmount,
                    Currency = intention.PaymentAsset,
                    CardToken = paymentDetails?.CardToken ?? String.Empty,
                    CryptoBuyId = intention.Id,
                    CryptoBuyClientId = Program.Settings.ServiceClientId,
                    CryptoBuyWalletId = Program.Settings.ServiceWalletId
                });

                intention.UnlimintDepositId = response.DepositId;

                if (response.Status == AddCardDepositResponse.StatusCode.Ok)
                {
                    _logger.LogInformation(
                        "Receiver successful response for intentionId {intentionId} from unlimint {response}",
                        intention.Id, response);
                    intention.Status = BuyStatus.PaymentCreated;
                    intention.DepositCheckoutLink = response.RedirectUrl;
                }
                else
                {
                    _logger.LogError(
                        "Receiver unsuccessful response for intentionId {intentionId} from unlimint {response}",
                        intention.Id, response);
                    intention.PaymentCreationErrorCode = response.Status;
                    intention.LastError = response.Status.ToString();
                    intention.Status = BuyStatus.Failed;
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When requesting unlimint payment for intention {intentionId}", intention.Id);
                throw;
            }
        }
    }
    
    
    
}
