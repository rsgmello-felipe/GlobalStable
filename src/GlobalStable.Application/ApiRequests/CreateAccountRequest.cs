namespace GlobalStable.Application.UseCases.Accounts;

public class CreateAccountRequest
{
    public string Name { get;  set; }

    public long CustomerId { get;  set; }

    public string Currency { get;  set; }

    public string? WalletAddress { get;  set; }
    
    public decimal WithdrawalPercentageFee { get;  set; }
    
    public decimal WithdrawalFlatFee { get;  set; }
    
    public decimal DepositPercentageFee { get;  set; }
    
    public decimal DepositFlatFee { get;  set; }

    public bool Enabled { get;  set; } = true;

}