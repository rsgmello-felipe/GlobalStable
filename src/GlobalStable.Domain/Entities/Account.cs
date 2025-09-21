namespace GlobalStable.Domain.Entities;

public class Account : EntityBase
{
    public string Name { get; private set; }

    public long CustomerId { get; private set; }

    public long CurrencyId { get; private set; }

    public string? WalletAddress { get; private set; }
    
    public decimal WithdrawalPercentageFee { get; private set; }
    
    public decimal WithdrawalFlatFee { get; private set; }
    
    public decimal DepositPercentageFee { get; private set; }
    
    public decimal DepositFlatFee { get; private set; }

    public bool Enabled { get; private set; } = true;

    public DateTimeOffset LastUpdatedAt { get; private set; }

    public string LastUpdatedBy { get; private set; }

    public Currency Currency { get; set; }

    public Account()
    {
    }

    public Account(
        string name,
        long customerId,
        long currencyId,
        decimal withdrawalPercentageFee,
        decimal withdrawalFlatFee,
        decimal depositPercentageFee,
        decimal depositFlatFee,
        string createdBy,
        string? walletAddress = null)
    {
        Name = name;
        CustomerId = customerId;
        CurrencyId = currencyId;
        WalletAddress = walletAddress;
        WithdrawalPercentageFee = withdrawalPercentageFee;
        WithdrawalFlatFee = withdrawalFlatFee;
        DepositPercentageFee = depositPercentageFee;
        DepositFlatFee = depositFlatFee;
        Enabled = true;
        CreatedAt = DateTimeOffset.Now;
        CreatedBy = createdBy;
        LastUpdatedAt = DateTimeOffset.Now;
        LastUpdatedBy = createdBy;
    }

    public void UpdateAccount(
        string lastUpdatedBy,
        string? name = null,
        string? walletAddress = null,
        bool? enabled = null)
    {
        Name = name ?? Name;
        WalletAddress = walletAddress ?? WalletAddress;
        Enabled = enabled ?? Enabled;
        LastUpdatedAt = DateTimeOffset.Now;
        LastUpdatedBy = lastUpdatedBy;
    }
}