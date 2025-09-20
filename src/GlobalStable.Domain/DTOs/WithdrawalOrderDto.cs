using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.DTOs;

public class WithdrawalOrderDto
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public decimal RequestedAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string ReceiverTaxId { get; set; }

    public string ReceiverAccountKey { get; set; }

    public string ReceiverName { get; set; }

    public string? WebhookUrl { get; set; }

    public string Currency { get; set; }

    public List<OrderHistoryDto> OrderHistory { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}


