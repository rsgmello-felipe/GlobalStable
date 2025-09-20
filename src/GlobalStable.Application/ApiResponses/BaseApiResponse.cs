using System.Diagnostics.CodeAnalysis;

namespace GlobalStable.Application.ApiResponses;

/// <summary>
/// Standardized API response structure.
/// </summary>
/// <typeparam name="T">Type of the response data.</typeparam>
[ExcludeFromCodeCoverage]
public class BaseApiResponse<T>
{
    public bool Status { get; set; }

    public T? Result { get; set; }

    public string? Message { get; set; }

    public BaseApiResponse(T? result, int statusCode = 200, string? msg = null)
    {
        Result = result;
        Status = statusCode >= 200 && statusCode < 300;
        Message = msg;
    }
}
