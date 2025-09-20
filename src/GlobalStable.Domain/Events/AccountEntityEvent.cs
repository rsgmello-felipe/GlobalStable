using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Events;

public class AccountEntityEvent
{
    public long Id { get; set; }

    public string Name { get; set; }

    public long CustomerId { get; set; }

    public long CurrencyId { get; set; }

    public decimal MaxAllowedDebt { get; set; }

    public string? WalletAddress { get; set; }

    public bool IsAutomaticApproval { get; set; }

    public bool AutoExecuteWithdrawal { get; set; }

    public bool Enabled { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid LastUpdatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LastUpdatedAt { get; set; }

    public FeeConfigDto? DepositFee { get; set; }

    public FeeConfigDto? WithdrawalFee { get; set; }

}

public class FeeConfigDto
{
    public TransactionOrderType OrderType { get; set; }

    public decimal FeePercentage { get; set; }

    public decimal FlatFee { get; set; }
}