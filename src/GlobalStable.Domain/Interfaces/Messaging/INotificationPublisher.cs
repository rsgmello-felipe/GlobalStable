using GlobalStable.Domain.Events;

namespace GlobalStable.Domain.Interfaces.Messaging;

public interface INotificationPublisher
{
    Task PublishDepositOrderFinishedAsync(
        DepositOrderNotificationEvent notificationEvent,
        string routingKey);

    Task PublishWithdrawalOrderFinishedAsync(
        WithdrawalOrderNotificationEvent notificationEvent,
        string routingKey);
}