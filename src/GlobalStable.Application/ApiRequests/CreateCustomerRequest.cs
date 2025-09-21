namespace GlobalStable.Application.ApiRequests;

public class CreateCustomerRequest
{
    public string Name { get;  set; }

    public string TaxId { get;  set; }

    public string Country { get;  set; }
    
    public decimal QuoteSpread { get;  set; }
}