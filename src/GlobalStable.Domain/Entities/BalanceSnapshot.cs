using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlobalStable.Domain.Entities;

public class BalanceSnapshot : EntityBase
{
    public long AccountId { get; private set; }

    public long CurrencyId { get; private set; }

    public Currency Currency { get; private set; } = null!;

    [Column(TypeName = "numeric(38, 18)")]
    public decimal Amount { get; private set; }


    public BalanceSnapshot(
        long accountId,
        long currencyId,
        decimal amount,
        DateTimeOffset createdAt)
    {
        AccountId = accountId;
        CurrencyId = currencyId;
        Amount = amount;
        CreatedAt = createdAt;
    }
}