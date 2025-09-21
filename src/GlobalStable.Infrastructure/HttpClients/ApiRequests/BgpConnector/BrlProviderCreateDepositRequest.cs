namespace GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;

public class BrlProviderCreateDepositRequest(
    long orderId,
    string currency,
    decimal amount,
    int expiration)
{
    public string OrderId { get; set; } = orderId.ToString();

    public string Currency { get; set; } = currency;

    public decimal Amount { get; set; } = amount;

    public int Expiration { get; set; } = expiration;
}