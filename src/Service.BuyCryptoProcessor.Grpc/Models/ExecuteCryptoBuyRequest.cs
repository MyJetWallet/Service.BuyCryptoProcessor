using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class ExecuteCryptoBuyRequest
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public string PaymentId { get; set; }
        [DataMember(Order = 3)] public string KeyId { get; set; }
        [DataMember(Order = 4)] public string SessionId { get; set; }
        [DataMember(Order = 5)] public string IpAddress { get; set; }
        [DataMember(Order = 6)] public string CardId { get; set; }
        [DataMember(Order = 7)] public string EncryptedData { get; set; }
    }
}