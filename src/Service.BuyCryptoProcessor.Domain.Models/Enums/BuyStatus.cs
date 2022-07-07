namespace Service.BuyCryptoProcessor.Domain.Models.Enums
{
    public enum BuyStatus
    {
        New = 0,
        ExecutionStarted = 1,
        PaymentCreated = 2,
        PaymentReceived = 3,
        ConversionExecuted = 4,
        Finished = 5,
        Failed = -1,
    }
}