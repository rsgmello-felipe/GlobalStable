namespace GlobalStable.Application.ApiResponses;

public class RefundDepositResponse
{
    public long DepositOrderId { get; set; }

    public string Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public RefundDepositResponse(
        long depositOrderId,
        string status,
        DateTimeOffset createdAt)
    {
        DepositOrderId = depositOrderId;
        Status = status;
        CreatedAt = createdAt;
    }
}