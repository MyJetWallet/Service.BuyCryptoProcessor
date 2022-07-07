using System.ServiceModel;
using System.Threading.Tasks;
using Service.BuyCryptoProcessor.Grpc.Models;

namespace Service.BuyCryptoProcessor.Grpc;

[ServiceContract]
public interface ICryptoBuyManager
{
    [OperationContract]
    Task<IntentionsResponse> GetIntentions(GetIntentionsRequest request);
        
    [OperationContract]
    Task<RetryCryptoBuyResponse> RetryCryptoBuy(RetryCryptoBuyRequest request);
}