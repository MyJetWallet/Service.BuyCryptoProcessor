using System.Runtime.Serialization;
using MyJetWallet.Circle.Models.Payments;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models.Enums;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class ExecuteCryptoBuyResponse
    {
        [DataMember(Order = 1)] public bool IsSuccess { get; set; }
        //[DataMember(Order = 2)] public AddCardDepositResponse.StatusCode ErrorCardCode { get; set; }
        [DataMember(Order = 3)] public CryptoBuyErrorCode ErrorCode { get; set; }
        [DataMember(Order = 4)] public PaymentErrorCode? PaymentExecutionErrorCode { get; set; }
        [DataMember(Order = 5)] public AddCardDepositResponse.StatusCode? PaymentCreationErrorCode { get; set; }

    }
}