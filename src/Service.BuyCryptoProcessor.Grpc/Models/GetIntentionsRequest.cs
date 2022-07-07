using System;
using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class GetIntentionsRequest
    {
        [DataMember(Order = 1)] public DateTime LastTs { get; set; }
        [DataMember(Order = 2)] public int BatchSize { get; set; }
        [DataMember(Order = 3)] public string WalletId { get; set; }
        [DataMember(Order = 4)] public string PaymentAsset { get; set; }
        [DataMember(Order = 5)] public string BuyAsset { get; set; }

        [DataMember(Order = 6)] public BuyStatus? BuyStatus { get; set; }
    }
}