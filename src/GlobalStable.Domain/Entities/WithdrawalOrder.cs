using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

/// <summary>
/// Represents a withdrawal order.
/// </summary>
public class WithdrawalOrder : EntityBase
{
    public long AccountId { get; private set; }
    
    public long CustomerId { get; private set; }

    public long StatusId { get; private set; }
    
    public string? StatusDescription { get; private set; }

    public decimal RequestedAmount { get; private set; }

    public decimal FeeAmount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public long CurrencyId { get; private set; }
    
    public string? E2EId { get; private set; }

    public string ReceiverName { get; private set; }
    
    public string? ReceiverTaxId { get; private set; }

    public string? ReceiverAccountKey { get; private set; }

    public string? ReceiverWalletAddress { get; private set; }

    public string? BlockchainNetworkId { get; private set; }
    
    public DateTimeOffset LastUpdatedAt { get; private set; }

    public string LastUpdatedBy { get; private set; }

    public ICollection<OrderHistory> OrderHistory { get; private set; } = new List<OrderHistory>();

    public Currency Currency { get; private set; }
    
    private WithdrawalOrder() { }

    public WithdrawalOrder(
        long accountId,
        long customerId,
        decimal requestedAmount,
        decimal feeAmount,
        decimal totalAmount,
        long currencyId,
        long statusId,
        string name,
        string? e2eId,
        string? receiverTaxId,
        string? receiverAccountKey,
        string? receiverWalletAddress,
        string? blockchainNetworkId,
        string createdBy)
    {
        AccountId = accountId;
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
        FeeAmount = feeAmount;
        TotalAmount = totalAmount;
        CurrencyId = currencyId;
        StatusId = statusId;
        ReceiverName = name;
        E2EId = e2eId;
        ReceiverTaxId = receiverTaxId;
        ReceiverAccountKey = receiverAccountKey;
        ReceiverWalletAddress = receiverWalletAddress;
        BlockchainNetworkId = blockchainNetworkId;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        LastUpdatedAt = DateTime.UtcNow;
        LastUpdatedBy = createdBy;

        OrderHistory = new List<OrderHistory>
        {
            new(
                withdrawalOrderId: Id,
                depositOrderOrderId: null,
                OrderType.Withdrawal,
                statusId,
                createdBy),
        };
    }

    /// <summary>
    /// Updates the status of the withdrawal order.
    /// </summary>
    /// <param name="newStatus">The new order status.</param>
    /// <param name="updatedBy">The user who performed the update.</param>
    /// <exception cref="InvalidOperationException">Thrown if the new status is the same as the current status.</exception>
    public void UpdateStatus(
        OrderStatus newStatus,
        string updatedBy,
        string? description = null)
    {
        if (newStatus == null)
        {
            throw new ArgumentNullException(nameof(newStatus), "New status cannot be null.");
        }

        if (newStatus.Id == StatusId)
        {
            throw new InvalidOperationException("The new status must be different from the current status.");
        }

        StatusId = newStatus.Id;
        LastUpdatedAt = DateTime.UtcNow;
        LastUpdatedBy = updatedBy;
        StatusDescription = description;

        // Register status update in history
        OrderHistory.Add(new OrderHistory(
            withdrawalOrderId: Id,
            null,
            OrderType.Withdrawal,
            newStatus.Id,
            updatedBy,
            description));
    }

    public void UpdateBankTransactionInformation(
        string? reason = null,
        string? E2eId = null)
    {
        E2EId = E2eId;
        StatusDescription = reason;
    }
}
