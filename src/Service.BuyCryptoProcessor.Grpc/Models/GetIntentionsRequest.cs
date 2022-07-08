using System;
using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class GetIntentionsRequest
    {
        [DataMember(Order = 1)] public DateTime LastSeen { get; set; }
        [DataMember(Order = 2)] public int Take { get; set; }
        [DataMember(Order = 3)] public string ClientId { get; set; }
        [DataMember(Order = 4)] public string PaymentAsset { get; set; }
        [DataMember(Order = 5)] public string BuyAsset { get; set; }
        [DataMember(Order = 6)] public BuyStatus? BuyStatus { get; set; }
        [DataMember(Order = 7)] public DateTime? CreationDateFrom { get; set; }
        [DataMember(Order = 8)] public DateTime? CreationDateTo { get; set; }
        [DataMember(Order = 9)] public string SearchText { get; set; }

    }
}