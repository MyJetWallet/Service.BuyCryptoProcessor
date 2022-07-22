using System;
using System.Runtime.Serialization;
using MyJetWallet.Circle.Models.Payments;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Domain.Models
{
    [DataContract]
    public class CryptoBuyIntention
    {
        public const string TopicName = "cryptobuy-intention";
        [DataMember(Order = 1)]public string Id { get; set; }
        [DataMember(Order = 2)]public string ClientId { get; set; }
        [DataMember(Order = 3)]public string BrandId { get; set; }
        [DataMember(Order = 4)]public string BrokerId { get; set; }
        [DataMember(Order = 5)]public string WalletId { get; set; }
        [DataMember(Order = 6)]public DateTime CreationTime { get; set; }
        [DataMember(Order = 7)]public BuyStatus Status { get; set; }
        [DataMember(Order = 8)]public WorkflowState WorkflowState { get; set; }
        [DataMember(Order = 9)]public string LastError { get; set; }
        [DataMember(Order = 10)]public int RetriesCount { get; set; }
        [DataMember(Order = 11)]public string ServiceWalletId { get; set; }

        //buy
        [DataMember(Order = 12)]public string DepositProfileId { get; set; }
        [DataMember(Order = 13)]public PaymentMethods PaymentMethod { get; set; }
        //[DataMember(Order = 14)]public string PaymentDetails { get; set; }
        [DataMember(Order = 15)]public decimal PaymentAmount { get; set; }
        [DataMember(Order = 16)]public string PaymentAsset { get; set; }
        [DataMember(Order = 17)]public decimal ProvidedCryptoAmount { get; set; }
        [DataMember(Order = 18)]public string ProvidedCryptoAsset { get; set; }
        
        [DataMember(Order = 19)]public long CircleDepositId { get; set; }
        [DataMember(Order = 20)]public string DepositOperationId { get; set; }
        [DataMember(Order = 21)]public DateTime DepositTimestamp { get; set; }
        [DataMember(Order = 22)]public string DepositIntegration { get; set; }
        [DataMember(Order = 23)]public string CardId { get; set; }
        [DataMember(Order = 24)]public string CardLast4 { get; set; }
        [DataMember(Order = 25)]public string DepositCheckoutLink { get; set; }
        
        //swap
        [DataMember(Order = 26)]public string SwapProfileId { get; set; }
        [DataMember(Order = 27)]public decimal BuyFeeAmount { get; set; }
        [DataMember(Order = 28)]public string BuyFeeAsset { get; set; }
        
        [DataMember(Order = 29)]public decimal BuyAmount { get; set; }
        [DataMember(Order = 30)]public string BuyAsset { get; set; }
        
        [DataMember(Order = 31)]public decimal FeeAmount { get; set; }
        [DataMember(Order = 32)]public string FeeAsset { get; set; }
        
        [DataMember(Order = 33)]public decimal SwapFeeAmount { get; set; }
        [DataMember(Order = 34)]public string SwapFeeAsset { get; set; }
        
        [DataMember(Order = 35)]public string PreviewQuoteId { get; set; }
        [DataMember(Order = 36)]public DateTime PreviewConvertTimestamp { get; set; }
        [DataMember(Order = 37)]public decimal QuotePrice { get; set; }
        [DataMember(Order = 38)]public string ExecuteQuoteId { get; set; }
        [DataMember(Order = 39)]public DateTime ExecuteTimestamp { get; set; }
        [DataMember(Order = 40)]public decimal Rate { get; set; }
        [DataMember(Order = 41)]public string PaymentProcessorRequestId { get; set; }
        //[DataMember(Order = 42)]public PaymentErrorCode? PaymentExecutionErrorCode { get; set; }
        [DataMember(Order = 43)]public AddCardDepositResponse.StatusCode? PaymentCreationErrorCode { get; set; }
        [DataMember(Order = 44)]public decimal PaymentAssetIndexPrice { get; set; }
        [DataMember(Order = 45)]public decimal BuyAssetIndexPrice { get; set; }
        [DataMember(Order = 46)]public decimal SwapFeeAssetIndexPrice { get; set; }
        [DataMember(Order = 47)]public decimal BuyFeeAssetIndexPrice { get; set; }
        [DataMember(Order = 48)]public decimal SwapFeeAmountConverted { get; set; }
        [DataMember(Order = 49)]public string SwapFeeAssetConverted { get; set; }
        [DataMember(Order = 50)]public long UnlimintDepositId { get; set; }
        [DataMember (Order = 51)] public PaymentProviderErrorCode PaymentErrorCode { get; set; }
    }
    
    
}