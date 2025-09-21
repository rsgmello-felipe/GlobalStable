namespace GlobalStable.Domain.Entities;

public class Customer : EntityBase
{
    public string Name { get; private set; }

    public string TaxId { get; private set; }

    public string Country { get; private set; }
    
    public decimal QuoteSpread { get; private set; }

    public bool Enabled { get; private set; }

    public Customer(){}
    
    public Customer(
        string name,
        string taxId,
        string country,
        decimal quoteSpread)
    {
        Name = name;
        TaxId = taxId;
        Country = country;
        QuoteSpread = quoteSpread;
        Enabled = true;
    }

    public void Update(
        string? name = null,
        decimal? quoteSpread = null,
        bool? enabled = null)
    {
        Name = name ?? Name;
        QuoteSpread = quoteSpread ?? QuoteSpread;
        Enabled = enabled ?? Enabled;
    }
}