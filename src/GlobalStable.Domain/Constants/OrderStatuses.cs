namespace GlobalStable.Domain.Constants;

public static class OrderStatuses
{
    public const string Created = "CREATED";
    public const string Accepted = "ACCEPTED";
    public const string Quoted = "QUOTED";
    public const string PendingDeposit = "PENDING_DEPOSIT";
    public const string PendingTreasury = "PENDING_TREASURY";
    public const string SentToConnector = "SENT_TO_CONNECTOR";
    public const string PendingInBank = "PENDING_IN_BANK";
    public const string Processing = "PROCESSING";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
    public const string Cancelled = "CANCELLED";
    public const string ProcessingRefund = "PROCESSING_REFUND";
    public const string Returned = "RETURNED";
    public const string Expired = "EXPIRED";
    public const string Blocked = "BLOCKED";
    public const string Rejected = "REJECTED";
}