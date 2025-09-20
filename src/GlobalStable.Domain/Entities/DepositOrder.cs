using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class DepositOrder : EntityBase
{
    public long CustomerId { get; private set; }

    public long AccountId { get; private set; }

    public decimal RequestedAmount { get; private set; }

    public decimal FeeAmount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public long CurrencyId { get; private set; }

    public Currency Currency { get; private set; }

    public long StatusId { get; private set; }

    public string? StatusDescription { get; private set; }

    public string? Name { get; private set; }

    public string? PixCopyPaste { get; private set; }

    public string? E2EId { get; private set; }

    public string? BankReference { get; private set; }

    public string WebhookUrl { get; private set; }
    
    public DateTimeOffset ExpireAt { get; private set; }

    public ICollection<OrderHistory> OrderHistory { get; private set; } = new List<OrderHistory>();

    public DateTimeOffset LastUpdatedAt { get; private set; }

    public string LastUpdatedBy { get; private set; }

    private DepositOrder() { }

    public DepositOrder(
        long customerId,
        long accountId,
        decimal requestedAmount,
        decimal feeAmount,
        decimal totalAmount,
        long currencyId,
        long statusId,
        string? bankReference,
        string webhookUrl,
        DateTimeOffset expireAt,
        string createdBy,
        string? pixCopyPaste = null,
        string? cvu = null,
        string? e2eId = null,
        string? statusDescription = null,
        string? name = null)
    {
        CustomerId = customerId;
        AccountId = accountId;
        RequestedAmount = requestedAmount;
        FeeAmount = feeAmount;
        TotalAmount = totalAmount;
        CurrencyId = currencyId;
        StatusId = statusId;
        StatusDescription = statusDescription;
        Name = name;
        E2EId = e2eId;
        PixCopyPaste = pixCopyPaste;
        BankReference = bankReference;
        WebhookUrl = webhookUrl;
        ExpireAt = expireAt;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        LastUpdatedAt = DateTime.UtcNow;
        LastUpdatedBy = createdBy;

        OrderHistory = new List<OrderHistory>
        {
            new(
                withdrawalOrderId: null,
                depositOrderOrderId: Id,
                TransactionOrderType.Deposit,
                statusId,
                createdBy),
        };
    }

    public void UpdateStatus(
        OrderStatus newStatus,
        string updatedBy,
        string? bankId = null,
        string? e2eId = null,
        string? statusDescription = null)
    {
        if (newStatus == null)
        {
            throw new ArgumentNullException(nameof(newStatus), "New status cannot be null.");
        }

        if (newStatus.Id == StatusId)
        {
            throw new InvalidOperationException("The new status must be different from the current status.");
        }

        E2EId = e2eId;
        StatusId = newStatus.Id;
        StatusDescription = statusDescription;
        LastUpdatedAt = DateTime.UtcNow;
        LastUpdatedBy = updatedBy;

        OrderHistory.Add(new OrderHistory(
            null,
            depositOrderOrderId: Id,
            TransactionOrderType.Deposit,
            newStatus.Id,
            updatedBy));
    }

    public void UpdateBankTransactionInformation(
        string? reason = null,
        string? E2eId = null,
        string? pixCopyPaste = null,
        string? cvu = null,
        string? referenceId = null)
    {
        E2EId = E2eId;
        StatusDescription = reason;
        BankReference = referenceId;
        PixCopyPaste = pixCopyPaste;
    }
}
