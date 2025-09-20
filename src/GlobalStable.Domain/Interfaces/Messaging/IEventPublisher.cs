namespace GlobalStable.Domain.Interfaces.Messaging;

public interface IEventPublisher
{
    Task PublishEvent<T>(T eventMessage, string routingKey);
}