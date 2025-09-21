namespace GlobalStable.Application.ApiRequests;

public abstract class UpdateCustomerRequest
{
    public string? Name { get; set; }
    public decimal? QuoteSpread { get; set; }
    public bool? Enabled { get; set; }
}