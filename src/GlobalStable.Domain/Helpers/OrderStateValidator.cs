namespace GlobalStable.Domain.Helpers;

public static class OrderStateValidator
{
    private static readonly Dictionary<long, HashSet<long>> AutomaticDepositTransitions = new()
    {
        [2] = new() { 7, 12, 13 },       // PENDING_DEPOSIT → EXPIRED, BLOCKED, COMPLETED
        [7] = new() { 10 },              // COMPLETED → PROCESSING_REFUND
        [10] = new() { 11, 7 },          // PROCESSING_REFUND → RETURNED, COMPLETED (REFUND_FAILED)
    };

    private static readonly Dictionary<long, HashSet<long>> AutomaticWithdrawalTransitions = new()
    {
        [1] = new() { 3 },               // CREATED → PENDING_APPROVAL
        [3] = new() { 4, 9 },            // PENDING_APPROVAL → SENT_TO_CONNECTOR, CANCELLED
        [4] = new() { 5, 8, 13 },        // SENT_TO_CONNECTOR → PENDING_IN_BANK, FAILED, BLOCKED
        [5] = new() { 6, 8 },            // PENDING_IN_BANK → PROCESSING, FAILED
        [6] = new() { 7, 8 },            // PROCESSING → COMPLETED, FAILED
        [7] = new() { 11 },              // COMPLETED → RETURNED
    };

    private static readonly Dictionary<long, HashSet<long>> ManualWithdrawalTransitions = new()
    {
        [1] = new() { 14 },               // CREATED → PENDING_TREASURY
        [14] = new() { 7, 15 },          // PENDING_TREASURY → COMPLETED, REJECTED
    };

    public static bool ValidAutomaticDepositTransition(long currentStatusId, long nextStatusId)
    {
        return AutomaticDepositTransitions.TryGetValue(currentStatusId, out var allowed)
               && allowed.Contains(nextStatusId);
    }

    public static bool ValidAutomaticWithdrawalTransition(long currentStatusId, long nextStatusId)
    {
        return AutomaticWithdrawalTransitions.TryGetValue(currentStatusId, out var allowed)
               && allowed.Contains(nextStatusId);
    }

    public static bool ValidManualWithdrawalTransition(long currentStatusId, long nextStatusId)
    {
        return ManualWithdrawalTransitions.TryGetValue(currentStatusId, out var allowed)
               && allowed.Contains(nextStatusId);
    }
}
