namespace GlobalStable.Application.ApiRequests;

public abstract class UpdateCustomerRequest
{
    public long CustomerId { get; set; }
    public string? Name { get; set; }
    public decimal? QuoteSpread { get; set; }
    public bool? Enabled { get; set; }
}