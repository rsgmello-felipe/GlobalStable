namespace GlobalStable.Domain.Constants;

public static class MessagingKeys
{
    public const string OrderDepositConfirmed = "order.deposit.confirmed";
    public const string NotificationDepositOrder = "notification.deposit.order";
    public const string NotificationWithdrawalOrder = "notification.withdrawal.order";
    public const string DepositOrderStatusUpdated = "deposit.order.status.updated";
    public const string WithdrawalOrderStatusUpdated = "withdrawal.order.status.updated";

    public const string TransactionCreate = "transaction.create";

    public const string ConnectorDepositUpdate = "connector.deposit.update";
    public const string ConnectorWithdrawalUpdate = "connector.withdrawal.update";

    public const string Audit = "audit";

    public const string GlobalStableTopicExchange = "global_stable_topic_exchange";
}