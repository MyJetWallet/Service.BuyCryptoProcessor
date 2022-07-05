using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class CreateCryptoBuyResponse
    {
        [DataMember(Order = 1)] public PaymentMethods PaymentMethod { get; set; }
        [DataMember(Order = 2)] public string PaymentDetails { get; set; }
        [DataMember(Order = 3)] public decimal PaymentAmount { get; set; }
        [DataMember(Order = 4)] public string PaymentAsset { get; set; }
        [DataMember(Order = 5)]public decimal BuyAmount { get; set; }
        [DataMember(Order = 6)]public string BuyAsset { get; set; }
        [DataMember(Order = 7)]public decimal FeeAmount { get; set; }
        [DataMember(Order = 8)]public string FeeAsset { get; set; }
        [DataMember(Order = 9)]public decimal Rate { get; set; }
        [DataMember(Order = 10)]public string PaymentId { get; set; }

    }
}