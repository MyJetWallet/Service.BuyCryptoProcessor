using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Circle.Settings.Services;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.BuyCryptoProcessor.Domain.Models;
using Service.BuyCryptoProcessor.Grpc;
using Service.BuyCryptoProcessor.Grpc.Models;
using Service.BuyCryptoProcessor.Postgres;
using Service.BuyCryptoProcessor.Settings;
using Service.Fees.Client;
using Service.Fees.Domain.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Converter.Grpc;
using Service.Liquidity.Converter.Grpc.Models;

namespace Service.BuyCryptoProcessor.Services
{
    public class CryptoBuyService: ICryptoBuyService
    {
        private readonly ILogger<CryptoBuyService> _logger;
        private readonly IDepositFeesClient _depositFees;
        private readonly IQuoteService _quoteService;
        private readonly ICircleDepositService _circleService;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public CryptoBuyService(ILogger<CryptoBuyService> logger, IDepositFeesClient depositFees, IQuoteService quoteService, ICircleDepositService circleService, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _logger = logger;
            _depositFees = depositFees;
            _quoteService = quoteService;
            _circleService = circleService;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<CreateCryptoBuyResponse> CreateCryptoBuy(CreateCryptoBuyRequest request)
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
                PaymentDetails = request.PaymentDetails,
                PaymentAmount = request.PaymentAmount,
                PaymentAsset = request.PaymentAsset,
                SwapProfileId = request.SwapFeeProfileId,
                BuyAsset = request.BuyAsset
            };
            
            var providedCryptoAsset = GetCryptoAsset(request.BuyAsset, request.PaymentMethod);
            var buyFees = _depositFees.GetDepositFees(request.BrokerId, request.DepositFeeProfileId, providedCryptoAsset);
            
            intention.BuyFeeAsset = buyFees.AssetId;
            intention.BuyFeeAmount = buyFees.FeeSizeType switch
            {
                FeeSizeType.Absolute => buyFees.FeeSizeAbsolute,
                FeeSizeType.Percentage => intention.BuyFeeAmount * buyFees.FeeSizeRelative / 100m,
                FeeSizeType.Composite => (intention.BuyFeeAmount * buyFees.FeeSizeRelative / 100m) + buyFees.FeeSizeAbsolute
            };
            
            var providedCryptoAmount = intention.PaymentAmount - intention.BuyFeeAmount;
            intention.ProvidedCryptoAmount = providedCryptoAmount;
            intention.ProvidedCryptoAsset = providedCryptoAsset;

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
                    PaymentMethod = PaymentMethods.CircleCard,
                    PaymentDetails = null,
                    PaymentAmount = 0,
                    PaymentAsset = null,
                    BuyAmount = 0,
                    BuyAsset = null,
                    FeeAmount = 0,
                    FeeAsset = null,
                    Rate = 0,
                    PaymentId = null
                };
            }
            intention.PreviewQuoteId = quote.Data.OperationId;
            intention.BuyAmount = quote.Data.ToAssetVolume;
            intention.SwapFeeAmount = quote.Data.FeeAmount;
            intention.SwapFeeAsset = quote.Data.FeeAsset;
            intention.PreviewConvertTimestamp = DateTime.UtcNow;
            intention.QuotePrice = quote.Data.Price;

            intention.Rate = intention.BuyAmount / intention.PaymentAmount;
            intention.FeeAsset = intention.BuyAsset;
            intention.FeeAmount = intention.BuyFeeAmount + intention.SwapFeeAmount * intention.Rate;

            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            await context.UpsertAsync(new[] {intention});

            return new CreateCryptoBuyResponse
            {
                PaymentMethod = intention.PaymentMethod,
                PaymentDetails = intention.PaymentDetails,
                PaymentAmount = intention.PaymentAmount,
                PaymentAsset = intention.PaymentAsset,
                BuyAmount = intention.BuyAmount,
                BuyAsset = intention.BuyAsset,
                FeeAmount = intention.FeeAmount,
                FeeAsset = intention.FeeAsset,
                Rate = intention.Rate,
                PaymentId = intention.Id
            };
        }

        public async Task<ExecuteCryptoBuyResponse> ExecuteCryptoBuy(ExecuteCryptoBuyRequest request)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var intention = await context.Intentions.FirstOrDefaultAsync(t => t.Id == request.PaymentId);
            if (intention == null)
            {
                return new ExecuteCryptoBuyResponse()
                {
                    IsSuccess = false
                };
            }
            var response = await _circleService.AddCirclePayment(new AddCardDepositRequest
            {
                BrokerId = intention.BrokerId,
                ClientId = Program.Settings.ServiceClientId,
                WalletId = Program.Settings.ServiceWalletId,
                RequestId = intention.Id,
                KeyId = request.KeyId,
                SessionId = request.SessionId,
                IpAddress = request.IpAddress,
                Amount = intention.BuyAmount,
                Currency = intention.ProvidedCryptoAsset,
                CardId = request.CardId,
                EncryptedData = request.EncryptedData,
                CryptoBuyId = intention.Id,
                BuyerClientId = intention.ClientId,
                BuyerWalletId = intention.WalletId
            });

            intention.CircleDepositId = response.DepositId;
            intention.CardId = request.CardId;
            
            await context.UpsertAsync(new[] {intention});
            
            return new ExecuteCryptoBuyResponse()
            {
                IsSuccess = response.Status == AddCardDepositResponse.StatusCode.Ok,
                CardCode = response.Status
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
                    FeeAmount = intention.FeeAmount,
                    FeeAsset = intention.FeeAsset
                }
            };

            if (intention.Status is BuyStatus.PaymentCreated)
                response.CheckoutUrl = intention.DepositCheckoutLink;

            if (intention.Status is BuyStatus.ConversionExecuted or BuyStatus.Finished)
                response.BuyInfo = new Buy
                {
                    BuyAmount = intention.BuyAmount,
                    BuyAsset = intention.BuyAsset,
                    Rate = intention.Rate
                };

            return response;
        }


        private static string GetCryptoAsset(string paymentAsset, PaymentMethods paymentMethod)
        {
            //TODO: get assets dictionary
            return "USDC";
        }
    }
    
}
