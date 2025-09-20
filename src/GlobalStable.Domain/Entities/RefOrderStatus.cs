namespace GlobalStable.Domain.Entities;

/// <summary>
/// Represents a reference table for order statuses.
/// </summary>
public class OrderStatus
{
    public long Id { get; private set; }

    public string Name { get; private set; }

    public OrderStatus() { }

    public OrderStatus(long id, string name)
    {
        Id = id;
        Name = name;
    }
}