using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Infrastructure.Messaging;
using FluentResults;
using GlobalStable.Application.UseCases.DepositUseCases;

namespace GlobalStable.BackgroundServices.Consumers;

public class DepositStatusUpdateConsumer(
    ILogger<DepositStatusUpdateConsumer> logger,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory serviceScopeFactory)
    : BaseRabbitMqConsumer<OrderEvent>(
        logger,
        rabbitMqConnection,
        queueName: MessagingKeys.DepositOrderStatusUpdated,
        exchangeName: MessagingKeys.GlobalStableTopicExchange,
        routingKey: MessagingKeys.DepositOrderStatusUpdated)
{
    protected override async Task<Result> HandleEventAsync(OrderEvent eventMessage)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<HandleDepositStatusUpdatedUseCase>();

        var result = await useCase.ExecuteAsync(eventMessage);

        if (result.IsFailed)
        {
            logger.LogError(
                "Failed to process deposit status update for DepositOrderId {Id}: {Errors}",
                eventMessage.OrderId,
                string.Join(" | ", result.Errors.Select(e => e.Message)));
            return result;
        }

        logger.LogInformation(
            "Successfully processed deposit status update for DepositOrderId {Id}",
            eventMessage.OrderId);

        return Result.Ok();
    }
}