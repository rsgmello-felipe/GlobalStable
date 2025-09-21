namespace GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;

public class BrlProviderCreateWithdrawalRequest
{
    public string OrderId { get; set; }

    public string Currency { get; set; }

    public decimal Amount { get; set; }

    public string ReceiverAccountKey { get; set; }

    public string ReceiverTaxId { get; set; }

    public string ReceiverName { get; set; }

    public BrlProviderCreateWithdrawalRequest(
        string orderId,
        string currency,
        decimal amount,
        string receiverAccountKey,
        string receiverTaxId,
        string receiverName)
    {
        OrderId = orderId;
        Currency = currency;
        Amount = amount;
        ReceiverAccountKey = receiverAccountKey;
        ReceiverTaxId = receiverTaxId;
        ReceiverName = receiverName;
    }
}