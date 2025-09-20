namespace GlobalStable.Domain.Events;

public class AuditEvent
{
    public required string UserId { get; init; }

    public long? CustomerId { get; init; }

    public required string Operation { get; init; }

    public long? EntityId { get; init; }

    public string? EntityTable { get; init; }

    public required string OriginApplication { get; init; }

    public required string RequestPath { get; init; }

    public required string ApplicationInstance { get; init; }

    public string? Data { get; init; }

    public required DateTime OperationTimestamp { get; init; } = DateTime.UtcNow;
}