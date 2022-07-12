using System.Collections.Generic;
using System.Runtime.Serialization;
using MyJetWallet.Circle.Models.Payments;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class CryptoBuyStatusResponse
    {
        [DataMember(Order = 1)]public string PaymentId { get; set; }
        [DataMember(Order = 2)] public ClientAction ClientAction { get; set; }
        [DataMember(Order = 3)] public BuyStatus Status { get; set; }
        [DataMember(Order = 4)]public Payment PaymentInfo { get; set; }
        [DataMember(Order = 5)]public Buy BuyInfo { get; set; }
        [DataMember(Order = 6)] public PaymentErrorCode? PaymentExecutionErrorCode { get; set; }
        [DataMember(Order = 7)] public AddCardDepositResponse.StatusCode? PaymentCreationErrorCode { get; set; }
    }

    [DataContract]
    public class ClientAction
    {
        [DataMember(Order = 1)] public string CheckoutUrl { get; set; }
        [DataMember(Order = 2)]public List<string> RedirectUrls { get; set; }
    }
    
    [DataContract]
    public class Payment
    {
        [DataMember(Order = 1)] public string PaymentAsset { get; set; }
        [DataMember(Order = 2)]public decimal PaymentAmount { get; set; }
        [DataMember(Order = 3)]public decimal FeeAmount { get; set; }
        [DataMember(Order = 4)]public string FeeAsset { get; set; }
    }
    
    [DataContract]
    public class Buy
    {
        [DataMember(Order = 1)]public decimal BuyAmount { get; set; }
        [DataMember(Order = 2)]public string BuyAsset { get; set; }
        [DataMember(Order = 3)]public decimal Rate { get; set; }
    }
}