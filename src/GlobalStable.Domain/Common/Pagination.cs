namespace GlobalStable.Domain.Common;

/// <summary>
/// Represents pagination details.
/// </summary>
public class Pagination
{
    /// <summary>
    /// Gets total number of records in the query.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// Gets current page number.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Gets number of records per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets total number of pages based on total records and page size.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Gets a value indicating whether indicates whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets a value indicating whether indicates whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pagination"/> class.
    /// </summary>
    /// <param name="totalItems"></param>
    /// <param name="page">Current page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    public Pagination(
        int totalItems,
        int page,
        int pageSize)
    {
        TotalItems = totalItems;
        Page = page;
        PageSize = pageSize;
    }
}