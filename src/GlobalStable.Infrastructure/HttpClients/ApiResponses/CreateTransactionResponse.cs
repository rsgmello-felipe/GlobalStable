namespace GlobalStable.Infrastructure.HttpClients.ApiResponses;

public class CreateTransactionResponse
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; }

    public long OrderId { get; set; }

    public string OrderType { get; set; }
}