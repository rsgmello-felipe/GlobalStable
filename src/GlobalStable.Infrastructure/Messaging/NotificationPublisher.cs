using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;

namespace GlobalStable.Infrastructure.Messaging;

public class NotificationPublisher(IEventPublisher eventPublisher) : INotificationPublisher
{
    public async Task PublishDepositOrderFinishedAsync(
        DepositOrderNotificationEvent notificationEvent,
        string routingKey)
    {
        await eventPublisher.PublishEvent(notificationEvent, routingKey);
    }

    public async Task PublishWithdrawalOrderFinishedAsync(
        WithdrawalOrderNotificationEvent notificationEvent,
        string routingKey)
    {
        await eventPublisher.PublishEvent(notificationEvent, routingKey);
    }
}