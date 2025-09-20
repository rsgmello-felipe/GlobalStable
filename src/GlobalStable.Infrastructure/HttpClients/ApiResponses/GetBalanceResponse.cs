namespace GlobalStable.Infrastructure.HttpClients.ApiResponses;

public class GetBalanceResponse
{
    public long AccountId { get; set; }

    public decimal Balance { get; set; }
}