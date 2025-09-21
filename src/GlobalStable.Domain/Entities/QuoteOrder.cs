using System.ComponentModel.DataAnnotations.Schema;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class QuoteOrder : EntityBase
{
    public long CustomerId { get; private set; }
    
    public long StatusId { get; private set; }
    
    public string? StatusDescription { get; private set; }
    public long BaseCurrencyId { get; private set; }

    public long QuoteCurrencyId { get; private set; }
    
    public Side Side { get; private set; }

    public decimal? BaseAmount { get; private set; }

    public decimal? QuoteAmount { get; private set; }

    public decimal? Price { get; private set; }
    
    public decimal? FeeAmount { get; private set; }
    
    public long BaseAccountId { get; private set; }

    public long QuoteAccountId { get; private set; }
    
    public DateTimeOffset LastUpdatedAt { get; private set; }
    
    public string LastUpdatedBy { get; private set; }
    
    public Currency BaseCurrency { get; set; }
    
    public Currency QuoteCurrency { get; set; }
    
    public Account BaseAccount { get; set; }
    
    public Account QuoteAccount { get; set; }
    
    public QuoteOrder(){}

    public QuoteOrder(
        long customerId,
        long statusId,
        long baseCurrencyId,
        long quoteCurrencyId,
        Side side,
        decimal? baseAmount,
        decimal? quoteAmount,
        decimal price,
        decimal feeAmount,
        long baseAccountId,
        long quoteAccountId,
        string? statusDescription = null)
    {
        CustomerId = customerId;
        StatusId = statusId;
        StatusDescription = statusDescription;
        BaseCurrencyId = baseCurrencyId;
        QuoteCurrencyId = quoteCurrencyId;
        Side = side;
        BaseAmount = baseAmount;
        QuoteAmount = quoteAmount;
        Price = price;
        FeeAmount = feeAmount;
        BaseAccountId = baseAccountId;
        QuoteAccountId = quoteAccountId;
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = "System";
        LastUpdatedAt = DateTimeOffset.UtcNow;
        LastUpdatedBy = "System";
    }
    
    public void Accept(
        long? statusId = null,
        string? statusDescription = null)
    {
        StatusId = statusId ?? StatusId;
        StatusDescription = statusDescription ?? StatusDescription;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        LastUpdatedBy = "System";
    }
}