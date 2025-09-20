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

    public const string GlobalStableNewAccount = "order_service.new_account";
    public const string NewAccount = "entity.account";
    public const string GlobalStableNewCurrency = "order_service.entity_currency";
    public const string NewCurrency = "entity.currency";

    public const string Audit = "audit";

    public const string BgxTopicExchange = "bgx_topic_exchange";
}