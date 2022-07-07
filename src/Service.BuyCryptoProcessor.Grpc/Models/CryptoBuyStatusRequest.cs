using System.Runtime.Serialization;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class CryptoBuyStatusRequest
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public string PaymentId { get; set; }
    }
}