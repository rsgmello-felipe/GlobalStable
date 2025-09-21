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
        long id,
        string name,
        long customerId,
        long currencyId,
        decimal withdrawalPercentageFee,
        decimal withdrawalFlatFee,
        decimal depositPercentageFee,
        decimal depositFlatFee,
        DateTimeOffset createdAt,
        string createdBy,
        DateTimeOffset lastUpdatedAt,
        string lastUpdatedBy,
        string? walletAddress = null)
    {
        Id = id;
        Name = name;
        CustomerId = customerId;
        CurrencyId = currencyId;
        WalletAddress = walletAddress;
        WithdrawalPercentageFee = withdrawalPercentageFee;
        WithdrawalFlatFee = withdrawalFlatFee;
        DepositPercentageFee = depositPercentageFee;
        DepositFlatFee = depositFlatFee;
        Enabled = true;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        LastUpdatedAt = lastUpdatedAt;
        LastUpdatedBy = lastUpdatedBy;
    }

    public void UpdateAccount(
        bool automaticApproval,
        bool automaticExecute,
        string name,
        decimal maxAllowedDebt,
        string? walletAddress,
        bool enabled,
        DateTimeOffset lastUpdatedAt,
        string lastUpdatedBy)
    {
        Name = name;
        WalletAddress = walletAddress;
        Enabled = enabled;
        LastUpdatedAt = lastUpdatedAt;
        LastUpdatedBy = lastUpdatedBy;
    }
}