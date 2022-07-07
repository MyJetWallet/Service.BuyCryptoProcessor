using System.Runtime.Serialization;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class RetryCryptoBuyRequest
    {
        [DataMember(Order = 1)] public string PaymentId { get; set; }
    }
}