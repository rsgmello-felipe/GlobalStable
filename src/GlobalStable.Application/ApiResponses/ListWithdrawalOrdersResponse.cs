using GlobalStable.Domain.Common;
using GlobalStable.Domain.DTOs;

namespace GlobalStable.Application.ApiResponses;

/// <summary>
/// Represents a paginated response for withdrawal orders.
/// </summary>
public class ListWithdrawalOrdersResponse
{
    /// <summary>
    /// List of withdrawal orders.
    /// </summary>
    public List<WithdrawalOrderResponse> Orders { get; set; }

    /// <summary>
    /// Pagination details.
    /// </summary>
    public Pagination Pagination { get; set; }

    public ListWithdrawalOrdersResponse(
        IEnumerable<WithdrawalOrderResponse> orders,
        int page,
        int pageSize,
        int totalItems)
    {
        Orders = orders.ToList();
        Pagination = new Pagination(totalItems, page, pageSize);
    }
}