using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Infrastructure.Messaging;
using FluentResults;
using GlobalStable.Application.UseCases.WithdrawalOrderUseCases;

namespace GlobalStable.BackgroundServices.Consumers;

public class ConnectorWithdrawalEventConsumer(
    ILogger<ConnectorWithdrawalEventConsumer> logger,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory serviceScopeFactory)
    : BaseRabbitMqConsumer<ConnectorWithdrawalEvent>(
        logger,
        rabbitMqConnection,
        queueName: MessagingKeys.ConnectorWithdrawalUpdate,
        exchangeName: MessagingKeys.GlobalStableTopicExchange,
        routingKey: MessagingKeys.ConnectorWithdrawalUpdate)
{
    protected override async Task<Result> HandleEventAsync(ConnectorWithdrawalEvent eventMessage)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<UpdateWithdrawalStatusFromConnectorUseCase>();

        var result = await useCase.ExecuteAsync(eventMessage);

        if (result.IsFailed)
        {
            logger.LogError(
                "Failed to process withdrawal status update for WithdrawalOrderId {Id}: {Errors}",
                eventMessage.WithdrawalOrderId,
                string.Join(" | ", result.Errors.Select(e => e.Message)));
        }

        logger.LogInformation(
            "Successfully processed withdrawal status update for WithdrawalOrderId {Id}",
            eventMessage.WithdrawalOrderId);

        return result;
    }
}