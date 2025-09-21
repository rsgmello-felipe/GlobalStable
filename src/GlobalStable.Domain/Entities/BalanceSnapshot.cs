using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalStable.Domain.Entities;

public class BalanceSnapshot : EntityBase
{
    public long AccountId { get; private set; }
    
    public long CustomerId { get; private set; }

    public decimal IntervalBalance { get; private set; }
    
    public decimal TotalBalance { get; private set; }

    public long? LastTransactionId { get; private set; }    
    
    public long? PreviousBalanceSnapshotId { get; private set; }

    public long CurrencyId { get; private set; }

    public Currency Currency { get; private set; } = null!;
    
    public BalanceSnapshot(){}

    public BalanceSnapshot(
        long accountId,
        long customerId,
        decimal intervalBalance,
        decimal totalBalance,
        long? lastTransactionId,
        long? previousBalanceSnapshotId,
        long currencyId,
        DateTimeOffset createdAt)
    {
        AccountId = accountId;
        CurrencyId = currencyId; 
        CustomerId = customerId;
        IntervalBalance = intervalBalance;
        TotalBalance = totalBalance;
        LastTransactionId = lastTransactionId;
        PreviousBalanceSnapshotId = previousBalanceSnapshotId;
        CreatedAt = createdAt;
        CreatedBy = "System";
    }
}