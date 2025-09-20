using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Domain.Interfaces.Messaging;

namespace GlobalStable.Infrastructure.Messaging;

public class TransactionEventPublisher(IEventPublisher eventPublisher) : ITransactionEventPublisher
{
    public Task PublishTransactionCreatedAsync(CreateTransactionEvent transactionEvent)
    {
        return eventPublisher.PublishEvent(transactionEvent, MessagingKeys.TransactionCreate);
    }
}