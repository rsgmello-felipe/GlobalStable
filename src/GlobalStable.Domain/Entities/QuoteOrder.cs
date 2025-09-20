using System.ComponentModel.DataAnnotations.Schema;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class QuoteOrder
{
    [ForeignKey(nameof(BaseCurrency))]
    public long BaseCurrencyId { get; private set; }

    public Project.Domain.Entities.Lookup.Currency BaseCurrency { get; private set; } = null!;

    [ForeignKey(nameof(QuoteCurrency))]
    public long QuoteCurrencyId { get; private set; }

    public Project.Domain.Entities.Lookup.Currency QuoteCurrency { get; private set; } = null!;

    public Side Side { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal? BaseAmount { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal? QuoteAmount { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public QuoteOrder(
        long customerId,
        long baseCurrencyId,
        long quoteCurrencyId,
        Side side,
        decimal? baseAmount = null,
        decimal? quoteAmount = null,
        string? description = null) : base(customerId, description)
    {
        BaseCurrencyId = baseCurrencyId;
        QuoteCurrencyId = quoteCurrencyId;
        Side = side;
        BaseAmount = baseAmount;
        QuoteAmount = quoteAmount;
    }

    public void Accept()
    {
        AcceptedAt = DateTimeOffset.UtcNow;
        SetStatus(OrderStatus.Validated);
    }
}