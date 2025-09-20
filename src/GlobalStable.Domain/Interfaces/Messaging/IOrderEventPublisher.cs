using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Messaging;

public interface IOrderEventPublisher
{
    Task PublishWithdrawalOrderEvent(WithdrawalOrder order);

    Task PublishDepositOrderEvent(DepositOrder order);
}