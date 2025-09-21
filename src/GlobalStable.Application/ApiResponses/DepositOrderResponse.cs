using GlobalStable.Domain.DTOs;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Application.ApiResponses;

/// <summary>
/// Represents the response for a deposit order.
/// </summary>
public class DepositOrderResponse
{
    public long Id { get; private set; }

    public long AccountId { get; private set; }

    public long CustomerId { get; private set; }

    public decimal RequestedAmount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string Currency { get; private set; }

    public string CreatedBy { get; private set; }


    public string? E2eId { get; private set; }
    
    public DateTimeOffset ExpireAt { get; private set; }

    public string? Status { get; private set; }

    public List<OrderHistoryDto> OrderHistory { get; private set; } = new();

    public DepositOrderResponse() { }

    public DepositOrderResponse(
        DepositOrder order,
        DateTimeOffset expireAt,
        string currencyName,
        Dictionary<long, string> statuses,
        string? pixCopyAndPaste = null,
        string? cvu = null)
    {
        Id = order.Id;
        AccountId = order.AccountId;
        CustomerId = order.CustomerId;
        RequestedAmount = order.RequestedAmount;
        TotalAmount = order.TotalAmount;
        E2eId = order.E2EId;
        Currency = currencyName;
        CreatedBy = order.CreatedBy;
        ExpireAt = expireAt;

        OrderHistory = order.OrderHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new OrderHistoryDto
            {
                StatusName = statuses.TryGetValue(h.StatusId, out var name) ? name : "Unknown",
                Description = h.StatusDescription,
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

    public DepositOrderResponse(DepositOrder order, string currencyCode, Dictionary<long, string> statuses)
    {
        Id = order.Id;
        AccountId = order.AccountId;
        CustomerId = order.CustomerId;
        RequestedAmount = order.RequestedAmount;
        TotalAmount = order.TotalAmount;
        CreatedBy = order.CreatedBy;
        Currency = currencyCode;
        E2eId = order.E2EId;
        ExpireAt = order.ExpireAt;

        OrderHistory = order.OrderHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new OrderHistoryDto
            {
                StatusName = statuses.TryGetValue(h.StatusId, out var name) ? name : "Unknown",
                Description = h.StatusDescription,
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
