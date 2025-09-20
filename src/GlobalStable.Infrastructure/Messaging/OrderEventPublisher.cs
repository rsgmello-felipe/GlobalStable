using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;

namespace GlobalStable.Infrastructure.Messaging;

public class OrderEventPublisher(IEventPublisher eventPublisher) : IOrderEventPublisher
{
    public async Task PublishWithdrawalOrderEvent(WithdrawalOrder withdrawalOrder)
    {
        var withdrawalEvent = new OrderEvent(withdrawalOrder.Id);
        await eventPublisher.PublishEvent(withdrawalEvent, MessagingKeys.WithdrawalOrderStatusUpdated);
    }

    public async Task PublishDepositOrderEvent(DepositOrder depositOrder)
    {
        var depositEvent = new OrderEvent(depositOrder.Id);
        await eventPublisher.PublishEvent(depositEvent, MessagingKeys.DepositOrderStatusUpdated);
    }
}