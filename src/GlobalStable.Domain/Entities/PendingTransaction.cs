using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Domain.Entities;

public class PendingTransaction : EntityBase
{
    public long AccountId { get; private set; }
    
    public long CustomerId { get; private set; }

    public TransactionType Type { get; private set; }

    public decimal Amount { get; private set; }
    
    public long CurrencyId { get; private set; }

    public long OrderId { get; private set; }
    
    public TransactionOrderType OrderType { get; private set; }
    
    public Currency Currency { get; private set; }

    public PendingTransaction(){}
    
    public PendingTransaction(
        long accountId,
        long customerId,
        TransactionType type,
        decimal amount,
        long currencyId,
        long orderId,
        TransactionOrderType orderType)
    {
        AccountId = accountId;
        CustomerId = customerId;
        CurrencyId = currencyId;
        Type = type;
        OrderType = orderType;
        Amount = amount;
        OrderId = orderId;
    }
}