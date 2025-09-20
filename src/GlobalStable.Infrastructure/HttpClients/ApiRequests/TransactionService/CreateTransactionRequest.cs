using System.ComponentModel.DataAnnotations;
using GlobalStable.Domain.Enums;

namespace GlobalStable.Infrastructure.HttpClients.ApiRequests.TransactionService;

public class CreateTransactionRequest
{
    /// <summary>
    /// Gets or sets account identifier.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// Gets or sets (example: "DEPOSIT", "WITHDRAWAL").
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets (example: "USD", "BRL").
    /// </summary>
    [MaxLength(10)]
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets order Id associated to this transaction.
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// Gets or sets order type associated to this transaction.
    /// </summary>
    public string OrderType { get; set; }

    public CreateTransactionRequest (){}

    public CreateTransactionRequest(
        long accountId,
        TransactionType transactionType,
        decimal amount,
        string currency,
        long orderId,
        string orderType)
    {
        AccountId = accountId;
        Type = transactionType;
        Amount = amount;
        Currency = currency;
        OrderId = orderId;
        OrderType = orderType;
    }
}