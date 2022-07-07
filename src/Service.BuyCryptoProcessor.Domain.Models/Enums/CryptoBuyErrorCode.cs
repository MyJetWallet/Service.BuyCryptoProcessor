
namespace Service.BuyCryptoProcessor.Domain.Models.Enums
{
    public enum CryptoBuyErrorCode
    {
        InternalServerError = 0,
        OperationNotFound = 1,
        ConverterError = 2,
        CircleError = 3,
    }
}