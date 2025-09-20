namespace GlobalStable.Domain.Events;

public class ConnectorDepositEvent
{
    public long DepositOrderId { get; set; }

    public decimal Amount { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }

    public string? BankReference { get; set; }

    public string? BankId { get; set; }

    public string Status { get; set; }

    public string? Reason { get; set; }

    public string? E2EId { get; set; }
}