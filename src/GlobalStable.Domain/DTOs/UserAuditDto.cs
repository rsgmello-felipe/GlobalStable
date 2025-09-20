namespace GlobalStable.Domain.DTOs;

public class UserAudit
{
    public string UserId { get; set; }

    public string? EventId { get; set; }

    public UserAudit(
        string userId,
        string? eventId = null)
    {
        UserId = userId;
        EventId = eventId;
    }
}