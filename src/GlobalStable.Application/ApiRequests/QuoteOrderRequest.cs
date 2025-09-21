using GlobalStable.Domain.Enums;

namespace GlobalStable.Application.ApiRequests;

public class QuoteOrderRequest
{
    public long BaseCurrencyId { get; set; }
    public long QuoteCurrencyId { get; set; }
    public Side Side { get; set; }
    public decimal? BaseAmount { get; set; }
    public decimal? QuoteAmount { get; set; }
    public long BaseAccountId { get; set; }
    public long QuoteAccountId { get; set; }
}