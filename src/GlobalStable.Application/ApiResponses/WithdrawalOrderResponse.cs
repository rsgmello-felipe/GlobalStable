using GlobalStable.Domain.DTOs;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Application.ApiResponses;

/// <summary>
/// Represents the response for a withdrawal order.
/// </summary>
public class WithdrawalOrderResponse
{
    /// <summary>
    /// The unique identifier of the withdrawal order.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The account ID associated with the withdrawal order.
    /// </summary>
    public long AccountId { get; set; }

    /// <summary>
    /// The customer ID associated with the withdrawal order.
    /// </summary>
    public long CustomerId { get; set; }

    /// <summary>
    /// The amount requested for withdrawal.
    /// </summary>
    public decimal RequestedAmount { get; set; }

    /// <summary>
    /// The amount requested for withdrawal + Fee.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// The TaxId of the receiver.
    /// </summary>
    public string? ReceiverTaxId { get; set; }

    /// <summary>
    /// The account address of the receiver (PIX KEY or CVU).
    /// </summary>
    public string? ReceiverAccountKey { get; set; }

    /// <summary>
    /// The Wallet Address of the receiver.
    /// </summary>
    public string? ReceiverWalletAddress { get; set; }

    /// <summary>
    /// The wallet blockchain.
    /// </summary>
    public string? ReceiverBlockchain { get; set; }

    /// <summary>
    /// The name of the receiver.
    /// </summary>
    public string? ReceiverName { get; set; }

    /// <summary>
    /// The user who created the order.
    /// </summary>
    public string? WebhhokUrl { get; set; }

    /// <summary>
    /// EndToEnd ID associated to this order.
    /// </summary>
    public string? E2eId { get; set; }

    /// <summary>
    /// The currency of the withdrawal.
    /// </summary>
    public string Currency { get; set; }

    public string Status { get; private set; }

    /// <summary>
    /// The date of the request.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The date of the last update.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }

    /// <summary>
    /// The status of the withdrawal order.
    /// </summary>
    public List<OrderHistoryDto> OrderHistory { get; set; }

    public WithdrawalOrderResponse() { }

    public WithdrawalOrderResponse(
        WithdrawalOrder order,
        string currencyCode,
        Dictionary<long, string> statuses)
    {
        Id = order.Id;
        AccountId = order.AccountId;
        CustomerId = order.CustomerId;
        RequestedAmount = order.RequestedAmount;
        TotalAmount = order.TotalAmount;
        ReceiverTaxId = order.ReceiverTaxId;
        ReceiverAccountKey = order.ReceiverAccountKey;
        ReceiverWalletAddress = order.ReceiverWalletAddress;
        ReceiverBlockchain = order.ReceiverBlockchain;
        ReceiverName = order.Name;
        E2eId = order.E2EId;
        WebhhokUrl = order.WebhookUrl;
        Currency = currencyCode;
        CreatedAt = order.CreatedAt;
        LastUpdatedAt = order.LastUpdatedAt;

        OrderHistory = order.OrderHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new OrderHistoryDto
            {
                StatusName = statuses.TryGetValue(h.StatusId, out var name) ? name : "Unknown",
                Description = h.Description,
                CreatedAt = h.CreatedAt,
            })
            .ToList();

        var latestStatus = order.OrderHistory
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefault();

        if (latestStatus != null)
        {
            Status = statuses.TryGetValue(latestStatus.StatusId, out var statusName) ? statusName : "Unknown";
        }
    }
}