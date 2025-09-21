using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class Transaction : EntityBase
{
    public long AccountId { get; private set; }
    
    public long CustomerId { get; private set; }

    public TransactionType Type { get; private set; }
    
    public decimal Amount { get; private set; }
    
    public long CurrencyId { get; private set; }
    
    public long OrderId { get; private set; }
    
    public TransactionOrderType OrderType { get; private set; }
    
    public Currency Currency { get; private set; } = null!;

    public Transaction(){}
    
    public Transaction(
        long accountId,
        long customerId,
        TransactionType type,
        decimal amount,
        long currencyId,
        long orderId,
        TransactionOrderType transactionOrderType)
    {
        AccountId = accountId;
        CustomerId = customerId;
        Type = type;
        Amount = amount;
        CurrencyId = currencyId;
        OrderId = orderId;
        OrderType = transactionOrderType;
    }
}