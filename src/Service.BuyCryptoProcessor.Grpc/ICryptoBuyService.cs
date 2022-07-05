using System.ServiceModel;
using System.Threading.Tasks;
using Service.BuyCryptoProcessor.Grpc.Models;

namespace Service.BuyCryptoProcessor.Grpc
{
    [ServiceContract]
    public interface ICryptoBuyService
    {
        [OperationContract]
        Task<CreateCryptoBuyResponse> CreateCryptoBuy(CreateCryptoBuyRequest request);
        
        [OperationContract]
        Task<ExecuteCryptoBuyResponse> ExecuteCryptoBuy(ExecuteCryptoBuyRequest request);
        
        [OperationContract]
        Task<CryptoBuyStatusResponse> GetCryptoBuyStatus(CryptoBuyStatusRequest request);

    }
}