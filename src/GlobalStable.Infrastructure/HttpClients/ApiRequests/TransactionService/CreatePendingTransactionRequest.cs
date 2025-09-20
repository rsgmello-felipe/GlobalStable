using System.ComponentModel.DataAnnotations;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;

public class CreatePendingTransactionRequest
{
    public long AccountId { get; private set; }

    public TransactionType Type { get; private set; }

    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; private set; }

    public long OrderId { get; private set; }

    public string OrderType { get; private set; }

    public CreatePendingTransactionRequest(WithdrawalOrder order)
    {
        AccountId = order.AccountId;
        Type = TransactionType.Debit;
        Amount = -Math.Abs(order.TotalAmount);
        Currency = order.Currency.Code;
        OrderId = order.Id;
        OrderType = nameof(TransactionOrderType.Withdrawal);
    }

    public CreatePendingTransactionRequest(
        long accountId,
        decimal amount,
        string currency,
        long orderId,
        TransactionType type,
        string orderType)
    {
        AccountId = accountId;
        Type = type;
        Amount = amount;
        Currency = currency;
        OrderId = orderId;
        OrderType = orderType;
    }
}