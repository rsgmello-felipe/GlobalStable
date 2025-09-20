using System.ComponentModel.DataAnnotations;

namespace GlobalStable.Application.ApiRequests;

public class UpdateManualWithdrawalOrderRequest
{
    [Required]
    public long OrderId { get; set; }

    [Required]
    public string Status { get; set; }

    public string? Reason { get; set; }

    public string? TransactionHash { get; set; }

    public long OrderCustomerId { get; set; }

    public long RequestingCustomerId { get; set; }

    public long AccountId { get; set; }
}