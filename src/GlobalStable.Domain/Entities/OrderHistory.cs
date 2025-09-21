using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

/// <summary>
/// Represents the history of status changes for an order.
/// </summary>
public class OrderHistory : EntityBase
{
    public long? WithdrawalOrderId { get; private set; }

    public long? DepositOrderId { get; private set; }
    
    public long? QuoteOrderId { get; private set; }

    public long StatusId { get; private set; }
    
    public string? StatusDescription { get; private set; }
    
    public OrderType OrderType { get; private set; }

    public OrderHistory() {}

    public OrderHistory(
        long? withdrawalOrderId,
        long? depositOrderOrderId,
        OrderType orderType,
        long statusId,
        string createdBy,
        string? statusDescription = null)
    {
        WithdrawalOrderId = withdrawalOrderId;
        DepositOrderId = depositOrderOrderId;
        OrderType = orderType;
        StatusId = statusId;
        CreatedBy = createdBy;
        StatusDescription = statusDescription;
    }
}