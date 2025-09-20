using GlobalStable.Domain.Entities;

namespace GlobalStable.Infrastructure.HttpClients.ApiRequests.BgpConnector;

public class BgpCreateDepositRequest
{
    public string OrderId { get; set; }

    public string Currency { get; set; }

    public decimal Amount { get; set; }

    public int Expiration { get; set; }

    public string? PayerFirstName { get; set; }

    public string? PayerLastName { get; set; }

    public string? PayerTaxId { get; set; }

    public string? IndirectMerchantCode { get; set; }

    public string? Document { get; set; }

    public string? NotificationUrl { get; set; }

    public BgpCreateDepositRequest(
        long orderId,
        string currency,
        decimal amount,
        int expiration,
        string? name,
        string? taxId,
        string notificationUrl)
    {
        OrderId = orderId.ToString();
        Currency = currency;
        Amount = amount;
        Expiration = expiration;
        PayerFirstName = name?.Split(' ').First();
        PayerLastName = name?.Split(' ').Last();
        PayerTaxId = taxId;
        IndirectMerchantCode = "123";
        Document = "test";
        NotificationUrl = notificationUrl;
    }
}