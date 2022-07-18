using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class ExecuteCryptoBuyRequest
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public string PaymentId { get; set; }
        [DataMember(Order = 3)] public PaymentMethods PaymentMethod { get; set; }
        [DataMember(Order = 4)] public CirclePaymentDetails CirclePaymentDetails { get; set; }
        [DataMember(Order = 5)] public UnlimintPaymentDetails UnlimintPaymentDetails { get; set; }
    }

    [DataContract]
    public class CirclePaymentDetails
    {
        [DataMember(Order = 1)] public string KeyId { get; set; }
        [DataMember(Order = 2)] public string SessionId { get; set; }
        [DataMember(Order = 3)] public string IpAddress { get; set; }
        [DataMember(Order = 4)] public string CardId { get; set; }
        [DataMember(Order = 5)] public string EncryptedData { get; set; }
    }
    
    [DataContract]
    public class UnlimintPaymentDetails
    {
        [DataMember(Order = 1)] public string MerchantId { get; set; }
        [DataMember(Order = 2)] public string CardToken { get; set; }
        [DataMember(Order = 3)] public string IpAddress { get; set; }
    }
}