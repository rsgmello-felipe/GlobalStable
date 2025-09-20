namespace GlobalStable.Domain.Entities;

public class EntityBase
{
    public long Id { get; protected set; }

    public string CreatedBy { get; protected set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}