namespace GlobalStable.Infrastructure.HttpClients.ApiResponses;

public class GetCurrencyResponse
{
    public string Name { get; set; }

    public string Code { get; set; }

    public int Precision { get; set; }

    public string Type { get; set; }

    public bool Enabled { get; set; }

    public List<CurrencyBlockchain> CurrenciesBlockchains { get; set; }
}

public class CurrencyBlockchain
{
    public Blockchain Blockchain { get; set; } = default!;
}

public class Blockchain
{
    public string Name { get; set; }

    public string Code { get; set; }

    public string? Regex { get; set; }

    public bool Enabled { get; set; } = true;

}