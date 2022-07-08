using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.BuyCryptoProcessor.Domain.Models.Enums;
using Service.BuyCryptoProcessor.Grpc;
using Service.BuyCryptoProcessor.Grpc.Models;
using Service.BuyCryptoProcessor.Postgres;

namespace Service.BuyCryptoProcessor.Services
{
    public class CryptoBuyManager : ICryptoBuyManager
    {
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;
        private readonly ILogger<CryptoBuyManager> _logger;

        public CryptoBuyManager(DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, ILogger<CryptoBuyManager> logger)
        {
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            _logger = logger;
        }

        public async Task<IntentionsResponse> GetIntentions(GetIntentionsRequest request)
        {
            try
            {
                await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
                var query = context.Intentions.AsQueryable();

                if (request.Take == 0)
                {
                    request.Take = 20;
                }

                if (request.LastSeen != DateTime.MinValue)
                {
                    query = query.Where(t => t.CreationTime < request.LastSeen);
                }

                if (!string.IsNullOrWhiteSpace(request.ClientId))
                {
                    query = query.Where(t => t.ClientId == request.ClientId);
                }

                if (!string.IsNullOrWhiteSpace(request.BuyAsset))
                {
                    query = query.Where(e => e.BuyAsset == request.BuyAsset);
                }

                if (!string.IsNullOrWhiteSpace(request.PaymentAsset))
                {
                    query = query.Where(e => e.PaymentAsset == request.PaymentAsset);
                }

                if (request.BuyStatus != null)
                {
                    query = query.Where(e => e.Status == request.BuyStatus);
                }
                
                if (request.CreationDateFrom != null)
                {
                    query = query.Where(t => t.CreationTime >= request.CreationDateFrom);
                }
                
                if (request.CreationDateTo != null)
                {
                    query = query.Where(t => t.CreationTime <= request.CreationDateTo);
                }

                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    query = query.Where(t => t.ClientId.Contains(request.SearchText) ||
                                             t.Id.Contains(request.SearchText) ||
                                             t.PreviewQuoteId.Contains(request.SearchText) ||
                                             t.ExecuteQuoteId.Contains(request.SearchText) ||
                                             t.DepositOperationId.Contains(request.SearchText) ||
                                             t.ClientId.Contains(request.SearchText) ||
                                             t.CircleRequestId.Contains(request.SearchText));
                }
                
                var intentions = await query
                    .OrderByDescending(e => e.CreationTime)
                    .Take(request.Take)
                    .ToListAsync();

                var response = new IntentionsResponse
                {
                    Success = true,
                    Intentions = intentions,
                    TsForNextQuery = intentions.Min(t => t.CreationTime)
                };

                _logger.LogInformation("Return GetIntentions response count items: {count}",
                    response.Intentions.Count);
                return response;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Cannot get GetIntentions take: {takeValue}, LastId: {LastId}",
                    request.Take, request.LastSeen);
                return new IntentionsResponse {Success = false, ErrorMessage = exception.Message};
            }
        }

        public async Task<RetryCryptoBuyResponse> RetryCryptoBuy(RetryCryptoBuyRequest request)
        {
            try
            {
                await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
                var intention = await context.Intentions.FirstOrDefaultAsync(t => t.Id == request.PaymentId);
                if (intention == null)
                {
                    return new RetryCryptoBuyResponse()
                    {
                        IsSuccess = false,
                        ErrorMessage = "Intention not found"
                    };
                }

                intention.WorkflowState = WorkflowState.Retrying;
                await context.UpsertAsync(new[] {intention});
                
                var response = new RetryCryptoBuyResponse
                {
                    IsSuccess = true
                };
                return response;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Cannot retry payment {paymentId}", request.PaymentId);
                return new RetryCryptoBuyResponse {IsSuccess = false, ErrorMessage = exception.Message};
            }        
        }
    }
}