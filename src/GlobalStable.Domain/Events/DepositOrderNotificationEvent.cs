namespace GlobalStable.Domain.Events;

public class DepositOrderNotificationEvent
{
    public long OrderId { get; set; }

    public string OrderType { get; set; }

    public long AccountId { get; set; }

    public long CustomerId { get; set; }

    public string? WebhookUrl { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset FinalizedAt { get; set; }

    public string? FailureReason { get; set; } = null;

    public DepositOrderNotificationEvent(
        long orderId,
        string orderType,
        long accountId,
        long customerId,
        string? webhookUrl,
        decimal amount,
        string currency,
        string status,
        DateTimeOffset finalizedAt,
        string? failureReason)
    {
        OrderId = orderId;
        OrderType = orderType;
        AccountId = accountId;
        CustomerId = customerId;
        WebhookUrl = webhookUrl;
        Amount = amount;
        Currency = currency;
        Status = status;
        FinalizedAt = finalizedAt;
        FailureReason = failureReason;
    }
}