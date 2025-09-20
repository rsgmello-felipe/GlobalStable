namespace GlobalStable.Domain.Entities;

public class PaymentDetails
{
    public string? AccountHolder { get; private set; }

    public string? AccountNumber { get; private set; }

    public string? BankName { get; private set; }

    public string? RoutingNumber { get; private set; }

    private PaymentDetails() {}

    public PaymentDetails(
        string? accountHolder,
        string? accountNumber,
        string? bankName,
        string? routingNumber)
    {
        AccountHolder = accountHolder;
        AccountNumber = accountNumber;
        BankName = bankName;
        RoutingNumber = routingNumber;
    }
}
