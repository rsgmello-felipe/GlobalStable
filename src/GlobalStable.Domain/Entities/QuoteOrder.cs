using System.ComponentModel.DataAnnotations.Schema;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class QuoteOrder : EntityBase
{
    public long CustomerId { get; private set; }
    
    public string StatusId { get; private set; }
    
    public string StatusDescription { get; private set; }
    public long BaseCurrencyId { get; private set; }

    public long QuoteCurrencyId { get; private set; }
    
    public Side Side { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal? BaseAmount { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal? QuoteAmount { get; private set; }

    [Column(TypeName = "numeric(38, 18)")]
    public decimal? Price { get; private set; }
    
    [Column(TypeName = "numeric(38, 18)")]
    public decimal? FeeAmount { get; private set; }
    
    public long BaseAccountId { get; private set; }

    public long QuoteAccountId { get; private set; }
    
    public DateTimeOffset LastUpdatedAt { get; private set; }
    
    public string LastUpdatedBy { get; private set; }
    
    public Currency BaseCurrency { get; private set; }
    
    public Currency QuoteCurrency { get; private set; }
    
    public QuoteOrder(){}

    public QuoteOrder(
        long customerId,
        long baseCurrencyId,
        long quoteCurrencyId,
        Side side,
        decimal? baseAmount = null,
        decimal? quoteAmount = null,
        string? description = null)
    {
        CustomerId = customerId;
        BaseCurrencyId = baseCurrencyId;
        QuoteCurrencyId = quoteCurrencyId;
        Side = side;
        BaseAmount = baseAmount;
        QuoteAmount = quoteAmount;
    }
}