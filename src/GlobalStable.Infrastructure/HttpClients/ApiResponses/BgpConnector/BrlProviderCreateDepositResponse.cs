namespace GlobalStable.Infrastructure.HttpClients.ApiResponses.BgpConnector;

public class BrlProviderCreateDepositResponse
{
    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public string? PixCopyPaste { get; set; }

    public string? Cvu { get; set; }

    public string? ReceiverName { get; set; }

    public string? ReceiverTaxId { get; set; }

    public string? ReceiverTaxIdType { get; set; }

    public DateTime ExpireAt { get; set; }

    public string Method { get; set; }

    public string OrderId { get; set; }

    public string ReferenceId { get; set; }
}
