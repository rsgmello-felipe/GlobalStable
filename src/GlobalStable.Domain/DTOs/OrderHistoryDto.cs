namespace GlobalStable.Domain.DTOs;

public class OrderHistoryDto
{
    public string StatusName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}