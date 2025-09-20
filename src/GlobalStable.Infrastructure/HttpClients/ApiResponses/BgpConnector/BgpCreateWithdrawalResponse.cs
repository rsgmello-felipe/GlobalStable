namespace GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;

public class BgpCreateWithdrawalResponse
{
    public string OrderId { get; set; } = string.Empty;

    public string? Status { get; set; }

    public string? Message { get; set; }
}