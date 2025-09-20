namespace GlobalStable.Domain.Events;

public class DepositConfirmedEvent
{
    public long DepositOrderId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public DateTime ConfirmedAt { get; set; }

    public string BankReference { get; set; }

    public string BankId { get; set; }

    public string E2EId { get; set; }
}