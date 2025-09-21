namespace GlobalStable.Application.ApiRequests;

public class UpdateAccountRequest
{
    public long AccountId { get; set; }
    public string? Name { get; set; }
    public bool? Enabled { get; set; }
    public string? WalletAddress { get; set; }
}