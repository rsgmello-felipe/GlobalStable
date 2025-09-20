using System.Text.Json.Serialization;

namespace GlobalStable.Domain.Events;

/// <summary>
/// Event triggered when a deposit order is created or updated.
/// </summary>
public class OrderEvent(long orderId)
{
    [JsonInclude]
    public long OrderId { get; private set; } = orderId;
}