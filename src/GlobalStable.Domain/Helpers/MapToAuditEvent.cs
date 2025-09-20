namespace GlobalStable.Domain.Helpers;

public static class MapToAuditEvent
{
    public static string MapMethodToOperation(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "DELETE" => "Delete",
            "GET" => "Query",
            _ => "Unknown",
        };
    }

    public static bool MapStatusCodeToStatus(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }
}