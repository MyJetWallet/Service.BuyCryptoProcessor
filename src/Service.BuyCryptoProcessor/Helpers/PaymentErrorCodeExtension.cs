using System;
using MyJetWallet.Circle.Models.Payments;
using Service.Bitgo.DepositDetector.Domain.Models;

namespace Service.BuyCryptoProcessor.Helpers
{
    public static class PaymentErrorCodeExtension
    {
        public static PaymentProviderErrorCode ToErrorCode(this PaymentErrorCode? code)
        {
            return code switch
            {
                PaymentErrorCode.PaymentFailed => PaymentProviderErrorCode.PaymentFailed,
                PaymentErrorCode.PaymentFraudDetected => PaymentProviderErrorCode.PaymentFraudDetected,
                PaymentErrorCode.PaymentDenied => PaymentProviderErrorCode.PaymentDenied,
                PaymentErrorCode.PaymentNotSupportedByIssuer => PaymentProviderErrorCode.PaymentNotSupportedByIssuer,
                PaymentErrorCode.PaymentNotFunded => PaymentProviderErrorCode.PaymentNotFunded,
                PaymentErrorCode.PaymentUnprocessable => PaymentProviderErrorCode.PaymentUnprocessable,
                PaymentErrorCode.PaymentStoppedByIssuer => PaymentProviderErrorCode.PaymentStoppedByIssuer,
                PaymentErrorCode.PaymentCanceled => PaymentProviderErrorCode.PaymentCanceled,
                PaymentErrorCode.PaymentReturned => PaymentProviderErrorCode.PaymentReturned,
                PaymentErrorCode.PaymentFailedBalanceCheck => PaymentProviderErrorCode.PaymentFailedBalanceCheck,
                PaymentErrorCode.CardFailed => PaymentProviderErrorCode.CardFailed,
                PaymentErrorCode.CardInvalid => PaymentProviderErrorCode.CardInvalid,
                PaymentErrorCode.CardAddressMismatch => PaymentProviderErrorCode.CardAddressMismatch,
                PaymentErrorCode.CardZipMismatch => PaymentProviderErrorCode.CardZipMismatch,
                PaymentErrorCode.CardCvvInvalid => PaymentProviderErrorCode.CardCvvInvalid,
                PaymentErrorCode.CardExpired => PaymentProviderErrorCode.CardExpired,
                PaymentErrorCode.CardLimitViolated => PaymentProviderErrorCode.CardLimitViolated,
                PaymentErrorCode.CardNotHonored => PaymentProviderErrorCode.CardNotHonored,
                PaymentErrorCode.CardCvvRequired => PaymentProviderErrorCode.CardCvvRequired,
                PaymentErrorCode.CreditCardNotAllowed => PaymentProviderErrorCode.CreditCardNotAllowed,
                PaymentErrorCode.CardAccountIneligible => PaymentProviderErrorCode.CardAccountIneligible,
                PaymentErrorCode.CardNetworkUnsupported => PaymentProviderErrorCode.CardNetworkUnsupported,
                PaymentErrorCode.ChannelInvalid => PaymentProviderErrorCode.ChannelInvalid,
                PaymentErrorCode.UnauthorizedTransaction => PaymentProviderErrorCode.UnauthorizedTransaction,
                PaymentErrorCode.BankAccountIneligible => PaymentProviderErrorCode.BankAccountIneligible,
                PaymentErrorCode.BankTransactionError => PaymentProviderErrorCode.BankTransactionError,
                PaymentErrorCode.InvalidAccountNumber => PaymentProviderErrorCode.InvalidAccountNumber,
                PaymentErrorCode.InvalidWireRtn => PaymentProviderErrorCode.InvalidWireRtn,
                PaymentErrorCode.InvalidAchRtn => PaymentProviderErrorCode.InvalidAchRtn,
                PaymentErrorCode.RefIdInvalid => PaymentProviderErrorCode.RefIdInvalid,
                PaymentErrorCode.AccountNameMismatch => PaymentProviderErrorCode.AccountNameMismatch,
                PaymentErrorCode.AccountNumberMismatch => PaymentProviderErrorCode.AccountNumberMismatch,
                PaymentErrorCode.AccountIneligible => PaymentProviderErrorCode.AccountIneligible,
                PaymentErrorCode.WalletAddressMismatch => PaymentProviderErrorCode.WalletAddressMismatch,
                PaymentErrorCode.CustomerNameMismatch => PaymentProviderErrorCode.CustomerNameMismatch,
                PaymentErrorCode.InstitutionNameMismatch => PaymentProviderErrorCode.InstitutionNameMismatch,
                PaymentErrorCode.VerificationFailed => PaymentProviderErrorCode.VerificationFailed,
                PaymentErrorCode.VerificationFraudDetected => PaymentProviderErrorCode.VerificationFraudDetected,
                PaymentErrorCode.VerificationDenied => PaymentProviderErrorCode.VerificationDenied,
                PaymentErrorCode.VerificationNotSupportedByIssuer => PaymentProviderErrorCode.VerificationNotSupportedByIssuer,
                PaymentErrorCode.VerificationStoppedByIssuer => PaymentProviderErrorCode.VerificationStoppedByIssuer,
                PaymentErrorCode.ThreeDSecureNotSupported => PaymentProviderErrorCode.ThreeDSecureNotSupported,
                PaymentErrorCode.ThreeDSecureRequired => PaymentProviderErrorCode.ThreeDSecureRequired,
                PaymentErrorCode.ThreeDSecureFailure => PaymentProviderErrorCode.ThreeDSecureFailure,
                PaymentErrorCode.ThreeDSecureActionExpired => PaymentProviderErrorCode.ThreeDSecureActionExpired,
                PaymentErrorCode.ThreeDSecureInvalidRequest => PaymentProviderErrorCode.ThreeDSecureInvalidRequest,
                PaymentErrorCode.CardRestricted => PaymentProviderErrorCode.CardRestricted,
                null => PaymentProviderErrorCode.OK,
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
            };
        }
        
        public static PaymentProviderErrorCode ToErrorCode(this MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode? code)
        {
            //TODO: Check codes with Andrew
            return code switch
            {
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.BankMalfunction => PaymentProviderErrorCode
                    .BankAccountIneligible,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.SystemMalfunction => PaymentProviderErrorCode
                    .PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.CancelledByCustomer => PaymentProviderErrorCode
                    .PaymentStoppedByIssuer,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.DeclinedByAntifraud => PaymentProviderErrorCode
                    .VerificationFraudDetected,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.DeclinedBy3DSecure => PaymentProviderErrorCode
                    .ThreeDSecureFailure,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.Only3DSecureTransactionsAllowed =>
                    PaymentProviderErrorCode.ThreeDSecureRequired,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.Availability3DSecureUnknown =>
                    PaymentProviderErrorCode.ThreeDSecureInvalidRequest,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.LimitReached => PaymentProviderErrorCode
                    .PaymentUnprocessable,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.RequestedOperationNotSupported =>
                    PaymentProviderErrorCode.PaymentUnprocessable,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.DeclinedByBank => PaymentProviderErrorCode
                    .BankAccountIneligible,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.CommonDeclineByBank => PaymentProviderErrorCode
                    .BankAccountIneligible,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.SoftDecline => PaymentProviderErrorCode
                    .BankAccountIneligible,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.InsufficientFunds => PaymentProviderErrorCode
                    .PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.CardLimitReached => PaymentProviderErrorCode
                    .PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.IncorrectCardData => PaymentProviderErrorCode
                    .CardInvalid,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.DeclinedByBankAntifraud =>
                    PaymentProviderErrorCode.VerificationFraudDetected,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.ConnectionProblem => PaymentProviderErrorCode
                    .PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.IncorrectPaymentData => PaymentProviderErrorCode
                    .PaymentUnprocessable,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.BitcoinNoPaymentReceived =>
                    PaymentProviderErrorCode.PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.BitcoinWrongPaymentReceived =>
                    PaymentProviderErrorCode.PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.BitcoinConfirmationsPaymentTimeout =>
                    PaymentProviderErrorCode.PaymentFailed,
                MyJetWallet.Unlimint.Models.Payments.PaymentErrorCode.MaximumAmountLimitExceeded =>
                    PaymentProviderErrorCode.PaymentFailed,

                null => PaymentProviderErrorCode.OK,
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
            };
        }
    }
}