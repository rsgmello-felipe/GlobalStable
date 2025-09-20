using GlobalStable.Domain.Events;

namespace GlobalStable.Domain.Interfaces.Messaging;

public interface ITransactionEventPublisher
{
    Task PublishTransactionCreatedAsync(CreateTransactionEvent transactionEvent);
}