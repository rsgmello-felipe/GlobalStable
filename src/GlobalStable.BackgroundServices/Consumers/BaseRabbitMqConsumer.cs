using System.Text;
using System.Text.Json;
using GlobalStable.Infrastructure.Messaging;
using FluentResults;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GlobalStable.BackgroundServices.Consumers;

public abstract class BaseRabbitMqConsumer<TEvent> : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private const int MaxRetries = 5;
    private const int RetryTtlMs = 10000;

    protected CancellationToken StoppingToken { get; private set; }

    protected BaseRabbitMqConsumer(
        ILogger logger,
        RabbitMqConnection rabbitMqConnection,
        string queueName,
        string exchangeName,
        string routingKey)
    {
        _logger = logger;
        _queueName = queueName;
        _exchangeName = exchangeName;
        _routingKey = routingKey;
        _channel = rabbitMqConnection.CreateChannel();

        ConfigureQueue();
    }

    private void ConfigureQueue()
    {
        var retryQueue = $"{_queueName}.retry";
        var dlqQueue = $"{_queueName}.dlq";

        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

        var mainQueueArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", _exchangeName },
            { "x-dead-letter-routing-key", $"{_routingKey}.dlq" },
        };
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs);
        _channel.QueueBind(_queueName, _exchangeName, _routingKey);

        var retryArgs = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", _exchangeName },
            { "x-dead-letter-routing-key", _routingKey },
            { "x-message-ttl", RetryTtlMs },
        };

        _channel.QueueDeclare(retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: retryArgs);
        _channel.QueueBind(retryQueue, _exchangeName, $"{_routingKey}.retry");

        _channel.QueueDeclare(dlqQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(dlqQueue, _exchangeName, $"{_routingKey}.dlq");

        _channel.BasicQos(0, 10, false);

        _logger.LogInformation(
            "RabbitMQ queue configured: {Queue}, Retry={RetryQueue}, DLQ={DlqQueue}",
            _queueName,
            retryQueue,
            dlqQueue);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StoppingToken = stoppingToken;
        _logger.LogInformation("{Queue} consumer started...", _queueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var retryCount = GetRetryCount(ea.BasicProperties);

                _logger.LogInformation("Message received on {queueName}: {message}", _queueName, message);

                TEvent? eventMessage;
                try
                {
                    eventMessage = JsonSerializer.Deserialize<TEvent>(
                        message,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "Invalid JSON. Sending message to DLQ directly.");
                    PublishToQueue($"{_routingKey}.dlq", message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (eventMessage == null)
                {
                    _logger.LogWarning("Message deserialized as null. Sending to DLQ.");
                    PublishToQueue($"{_routingKey}.dlq", message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var result = await HandleEventAsync(eventMessage);

                if (result.IsSuccess)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    HandleFailure(message, retryCount);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error on {queueName}", _queueName);
                HandleFailure(Encoding.UTF8.GetString(ea.Body.ToArray()), GetRetryCount(ea.BasicProperties));
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    private void HandleFailure(string message, int retryCount)
    {
        if (retryCount < MaxRetries)
        {
            PublishToQueue($"{_routingKey}.retry", message, retryCount + 1);
        }
        else
        {
            _logger.LogWarning("Message exceeded {MaxRetries} retries. Sending to DLQ.", MaxRetries);
            PublishToQueue($"{_routingKey}.dlq", message);
        }
    }

    private int GetRetryCount(IBasicProperties props)
    {
        if (props?.Headers != null &&
            props.Headers.TryGetValue("x-retry-count", out var value))
        {
            if (value is byte[] bytes && int.TryParse(Encoding.UTF8.GetString(bytes), out int retryCount))
                return retryCount;

            if (value is int intVal)
                return intVal;
        }

        return 0;
    }

    private void PublishToQueue(string routingKey, string message, int retryCount = 0)
    {
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.Headers = new Dictionary<string, object>
        {
            { "x-retry-count", retryCount },
        };

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: props,
            body: Encoding.UTF8.GetBytes(message));

        _logger.LogWarning(
            "Message republished to {RoutingKey} with RetryCount={RetryCount}",
            routingKey,
            retryCount);
    }

    protected abstract Task<Result> HandleEventAsync(TEvent eventMessage);

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{QueueName} consumer is shutting down...", _queueName);
        _channel?.Close();
        _channel?.Dispose();
        await base.StopAsync(stoppingToken);
    }
}
