namespace GlobalStable.Domain.Helpers;

public static class OrderStatusDictionaryExtensions
{
    public static bool TryGetValueByName(this Dictionary<long, string> statuses, string name, out long statusId)
    {
        foreach (var kvp in statuses)
        {
            if (kvp.Value.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                statusId = kvp.Key;
                return true;
            }
        }

        statusId = 0;
        return false;
    }
}
