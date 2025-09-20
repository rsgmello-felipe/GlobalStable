using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;

namespace Project.Domain.Entities.Core;

public class Transaction
{
    [Key]
    public long Id { get; private set; }

    [ForeignKey(nameof(Account))]
    public long AccountId { get; private set; }
    public Account Account { get; private set; } = null!;

    [ForeignKey(nameof(Currency))]
    public long CurrencyId { get; private set; }
    public Currency Currency { get; private set; } = null!;

    public TransactionType Type { get; private set; }
    public TransactionOrderType OrderType { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal Amount { get; private set; }

    // Composite reference to the originating order (DepositOrder, WithdrawalOrder, QuoteOrder)
    public long OrderId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Transaction(long accountId, long currencyId, TransactionType type, TransactionOrderType orderType, decimal amount, long orderId)
    {
        AccountId = accountId;
        CurrencyId = currencyId;
        Type = type;
        OrderType = orderType;
        Amount = amount;
        OrderId = orderId;
    }
}