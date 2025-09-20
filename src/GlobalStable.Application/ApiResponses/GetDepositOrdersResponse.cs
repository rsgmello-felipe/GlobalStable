using GlobalStable.Domain.Common;

namespace GlobalStable.Application.ApiResponses;

/// <summary>
/// Represents a paginated response for deposit orders.
/// </summary>
public class GetDepositOrdersResponse
{
    /// <summary>
    /// Gets or sets list of deposit orders.
    /// </summary>
    public List<DepositOrderResponse> Orders { get; set; }

    /// <summary>
    /// Gets or sets pagination details.
    /// </summary>
    public Pagination Pagination { get; set; }

    public GetDepositOrdersResponse(
        IEnumerable<DepositOrderResponse> orders,
        int page,
        int pageSize,
        int totalItems)
    {
        Orders = orders.ToList();
        Pagination = new Pagination(
            totalItems,
            page,
            pageSize);
    }
}