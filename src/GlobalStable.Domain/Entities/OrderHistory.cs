using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

/// <summary>
/// Represents the history of status changes for an order.
/// </summary>
public class OrderHistory : EntityBase
{
    public long? WithdrawalOrderId { get; private set; }

    public long? DepositOrderId { get; private set; }

    public TransactionOrderType TransactionOrderType { get; private set; }

    public long StatusId { get; private set; }

    public string? Description { get; private set; }

    public OrderHistory() {}

    public OrderHistory(
        long? withdrawalOrderId,
        long? depositOrderOrderId,
        TransactionOrderType transactionOrderType,
        long statusId,
        string createdBy,
        string? description = null)
    {
        WithdrawalOrderId = withdrawalOrderId;
        DepositOrderId = depositOrderOrderId;
        TransactionOrderType = transactionOrderType;
        StatusId = statusId;
        CreatedBy = createdBy;
        Description = description;
    }
}