using System.Text;
using System.Text.Json;
using GlobalStable.Domain.Constants;
using GlobalStable.Domain.Interfaces.Messaging;
using GlobalStable.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace GlobalStable.Infrastructure.Messaging;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        RabbitMqConnection rabbitMqConnection)
    {
        _logger = logger;
        _channel = rabbitMqConnection.CreateChannel();

        // Declare exchange only once
        _channel.ExchangeDeclare(
            exchange: MessagingKeys.GlobalStableTopicExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Enable Publisher Confirms
        _channel.ConfirmSelect();

        // Retry with exponential backoff
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Attempt {RetryCount} failed. Retrying in {TimeSpan}. Error: {ExceptionMessage}",
                        retryCount,
                        timeSpan,
                        exception.Message);
                });
    }

    public async Task PublishEvent<T>(T eventMessage, string routingKey)
    {
        var messageBody = JsonSerializer.Serialize(eventMessage, JsonOptions);
        var body = Encoding.UTF8.GetBytes(messageBody);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.AppId = ServiceIdentity.ServiceName;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.MessageId = Guid.NewGuid().ToString();
        properties.CorrelationId = properties.MessageId;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            _channel.BasicPublish(
                exchange: MessagingKeys.GlobalStableTopicExchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            // Wait for broker confirmation
            if (!_channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
            {
                throw new Exception(
                    $"Broker did not confirm the publication of message {properties.MessageId}.");
            }

            _logger.LogInformation(
                "Event published to {RoutingKey} | MessageId={MessageId} | Size={Size} bytes",
                routingKey,
                properties.MessageId,
                body.Length);

            await Task.CompletedTask;
        });
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while closing RabbitMQ channel on dispose.");
        }
    }
}
