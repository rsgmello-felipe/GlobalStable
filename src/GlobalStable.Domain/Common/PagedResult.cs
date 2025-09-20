namespace GlobalStable.Domain.Common;

/// <summary>
/// Represents a paginated result.
/// </summary>
/// <typeparam name="T">Type of the data.</typeparam>
public class PagedResult<T>
{
    /// <summary> Gets or sets the list of items. </summary>
    public IEnumerable<T> Data { get; set; }

    /// <summary> Gets or sets the pagination details. </summary>
    public Pagination Pagination { get; set; }

    public PagedResult(IEnumerable<T> data, Pagination pagination)
    {
        Data = data;
        Pagination = pagination;
    }
}