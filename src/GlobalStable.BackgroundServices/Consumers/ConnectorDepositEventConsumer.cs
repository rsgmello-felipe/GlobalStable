using GlobalStable.Application.UseCases.Deposit;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Infrastructure.Messaging;
using FluentResults;

namespace GlobalStable.BackgroundServices.Consumers;

public class ConnectorDepositEventConsumer(
    ILogger<ConnectorDepositEventConsumer> logger,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory serviceScopeFactory)
    : BaseRabbitMqConsumer<ConnectorDepositEvent>(
        logger,
        rabbitMqConnection,
        queueName: MessagingKeys.ConnectorDepositUpdate,
        exchangeName: MessagingKeys.BgxTopicExchange,
        routingKey: MessagingKeys.ConnectorDepositUpdate)
{
    protected override async Task<Result> HandleEventAsync(ConnectorDepositEvent eventMessage)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<UpdateDepositStatusFromConnectorUseCase>();

        var result = await useCase.ExecuteAsync(eventMessage);

        if (result.IsFailed)
        {
            logger.LogError(
                "Failed to process deposit status update for DepositOrderId {Id}: {Errors}",
                eventMessage.DepositOrderId,
                string.Join(" | ", result.Errors.Select(e => e.Message)));
            return result;
        }

        logger.LogInformation(
            "Successfully processed deposit status update for DepositOrderId {Id}",
            eventMessage.DepositOrderId);

        return Result.Ok();
    }
}