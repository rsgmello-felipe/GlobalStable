using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Events;

public class CreateTransactionEvent
{
    public long CustomerId { get; set; }

    public long AccountId { get; set; }

    public string Type { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public string? ReferenceId { get; set; }

    public string? Description { get; set; }

    public string? E2EId { get; set; }

    public long OrderId { get; set; }

    public string OrderType { get; set; }

    public CreateTransactionEvent(
        long customerId,
        long accountId,
        long orderId,
        string type,
        decimal amount,
        string currency,
        string orderType,
        string? description,
        string? e2EId = null,
        string? referenceId = null)
    {
        CustomerId = customerId;
        AccountId = accountId;
        Type = type;
        Amount = amount;
        Currency = currency;
        ReferenceId = referenceId;
        OrderType = orderType;
        Description = description;
        E2EId = e2EId;
        OrderId = orderId;
    }
}