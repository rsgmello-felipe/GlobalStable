namespace GlobalStable.Domain.Entities;

public class EntityBase
{
    public long Id { get; protected set; }

    public string CreatedBy { get; protected set; } = "System";

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}