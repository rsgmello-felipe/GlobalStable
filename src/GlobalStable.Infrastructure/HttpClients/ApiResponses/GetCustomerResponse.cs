namespace GlobalStable.Infrastructure.HttpClients.ApiResponses;

public class GetCustomerResponse
{
    public long TypeId { get; set; }

    public required string Name { get; set; }

    public required string TaxId { get; set; }

    public required string Country { get; set; }

    public bool ChildEnabled { get; set; }

    public int ChildDepth { get; set; }

    public long? ParentId { get; set; }

    public bool Enabled { get; set; }
}
