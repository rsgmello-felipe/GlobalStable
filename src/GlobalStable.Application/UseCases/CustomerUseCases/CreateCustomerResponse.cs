namespace GlobalStable.Application.UseCases.CustomerUseCases;

public class CreateCustomerResponse
{
    public long Id { get; private set; }
    
    public string Name { get; private set; }

    public string TaxId { get; private set; }

    public string Country { get; private set; }
    
    public decimal QuoteSpread { get; private set; }
    
    public string ApiKey { get; private set; }
    
    public CreateCustomerResponse(
        long id,
        string name,
        string taxId,
        string country,
        decimal quoteSpread,
        string apiKey)
    {
        Id = id;
        Name = name;
        TaxId = taxId;
        Country = country;
        QuoteSpread = quoteSpread; 
        ApiKey = apiKey;
    }
}