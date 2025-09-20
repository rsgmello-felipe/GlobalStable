using GlobalStable.Application.UseCases.Withdrawal;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Events;
using GlobalStable.Infrastructure.Messaging;
using FluentResults;

namespace GlobalStable.BackgroundServices.Consumers;

public class WithdrawalStatusUpdateConsumer(
    ILogger<WithdrawalStatusUpdateConsumer> logger,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory serviceScopeFactory)
    : BaseRabbitMqConsumer<OrderEvent>(
        logger,
        rabbitMqConnection,
        queueName: MessagingKeys.WithdrawalOrderStatusUpdated,
        exchangeName: MessagingKeys.BgxTopicExchange,
        routingKey: MessagingKeys.WithdrawalOrderStatusUpdated)
{
    protected override async Task<Result> HandleEventAsync(OrderEvent eventMessage)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<HandleWithdrawalStatusUpdatedUseCase>();

        var result = await useCase.ExecuteAsync(eventMessage);

        if (result.IsFailed)
        {
            logger.LogError(
                "Failed to process withdrawal status update for WithdrawalOrderId {Id}: {Errors}",
                eventMessage.OrderId,
                string.Join(" | ", result.Errors.Select(e => e.Message)));
            return result;
        }

        logger.LogInformation(
            "Successfully processed withdrawal status update for WithdrawalOrderId {Id}",
            eventMessage.OrderId);

        return Result.Ok();
    }
}