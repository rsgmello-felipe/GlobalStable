using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class PendingTransaction
{
    [Key]
    public long Id { get; private set; }

    [ForeignKey(nameof(Account))]
    public long AccountId { get; private set; }

    public Project.Domain.Entities.Core.Account Account { get; private set; } = null!;

    [ForeignKey(nameof(Currency))]
    public long CurrencyId { get; private set; }

    public Currency Currency { get; private set; } = null!;

    public TransactionType Type { get; private set; }

    public TransactionOrderType OrderType { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal Amount { get; private set; }

    public long OrderId { get; private set; }


    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;


    public PendingTransaction(
        long accountId,
        long currencyId,
        TransactionType type,
        TransactionOrderType orderType,
        decimal amount,
        long orderId,
        DateTimeOffset? expiresAt = null)
    {
        AccountId = accountId;
        CurrencyId = currencyId;
        Type = type;
        OrderType = orderType;
        Amount = amount;
        OrderId = orderId;
    }
}