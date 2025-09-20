using GlobalStable.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Domain.Entities;

/// <summary>
/// Represents fee configuration for operations.
/// </summary>
public class FeeConfig : EntityBase
{
    public long AccountId { get; private set; }

    public TransactionOrderType TransactionOrderType { get; private set; }

    [Precision(5, 4)]
    public decimal FeePercentage { get; private set; }

    public decimal FlatFee { get; private set; }

    public bool Enabled { get; private set; } = true;

    public DateTimeOffset LastUpdatedAt { get; private set; }

    public string LastUpdatedBy { get; private set; }

    public FeeConfig() { }

    public FeeConfig(
        long accountId,
        TransactionOrderType transactionOrderType,
        decimal feePercentage,
        decimal flatFee,
        string createdBy)
    {
        AccountId = accountId;
        TransactionOrderType = transactionOrderType;
        FeePercentage = feePercentage;
        FlatFee = flatFee;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        LastUpdatedBy = createdBy;
    }

    public void Disable(string updatedBy)
    {
        Enabled = false;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        LastUpdatedBy = updatedBy;
    }
}