using System.Runtime.Serialization;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class ExecuteCryptoBuyResponse
    {
        [DataMember(Order = 1)]public bool IsSuccess { get; set; }
        [DataMember(Order = 2)]public AddCardDepositResponse.StatusCode CardCode { get; set; }
    }
}