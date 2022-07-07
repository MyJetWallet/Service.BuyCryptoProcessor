using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class RetryCryptoBuyResponse
    {
        [DataMember(Order = 1)] public bool IsSuccess { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
    
}