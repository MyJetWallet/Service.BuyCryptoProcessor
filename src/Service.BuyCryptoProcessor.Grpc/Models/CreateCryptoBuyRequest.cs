using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class CreateCryptoBuyRequest
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public string BrandId { get; set; }
        [DataMember(Order = 3)] public string BrokerId { get; set; }
        [DataMember(Order = 4)] public string WalletId { get; set; }
        
        [DataMember(Order = 5)] public PaymentMethods PaymentMethod { get; set; }
        [DataMember(Order = 6)] public string PaymentDetails { get; set; }
        [DataMember(Order = 7)] public decimal PaymentAmount { get; set; }
        [DataMember(Order = 8)] public string PaymentAsset { get; set; }
        
        [DataMember(Order = 9)] public string BuyAsset { get; set; }
        [DataMember(Order = 10)] public string DepositFeeProfileId { get; set; }
        [DataMember(Order = 11)] public string SwapFeeProfileId { get; set; }
    }
}