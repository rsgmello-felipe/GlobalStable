namespace GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;

public class BgpCreateWithdrawalRequest
{
    public string OrderId { get; set; }

    public string Currency { get; set; }

    public decimal Amount { get; set; }

    public string ReceiverAccountKey { get; set; }

    public string ReceiverTaxId { get; set; }

    public string ReceiverFirstName { get; set; }

    public string ReceiverLastName { get; set; }

    public string NotificationUrl { get; set; }

    public BgpCreateWithdrawalRequest(
        string orderId,
        string currency,
        decimal amount,
        string receiverAccountKey,
        string receiverTaxId,
        string receiverFirstName,
        string receiverLastName,
        string notificationUrl)
    {
        OrderId = orderId;
        Currency = currency;
        Amount = amount;
        ReceiverAccountKey = receiverAccountKey;
        ReceiverTaxId = receiverTaxId;
        ReceiverFirstName = receiverFirstName;
        ReceiverLastName = receiverLastName;
        NotificationUrl = notificationUrl;
    }
}