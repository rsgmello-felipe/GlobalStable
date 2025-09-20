using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace GlobalStable.Infrastructure.Messaging;

public class RabbitMqConnection : IDisposable
{
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMqConnection(
        string? host,
        string? username,
        string? password,
        ILogger<RabbitMqConnection> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _factory = new ConnectionFactory
        {
            HostName = host,
            UserName = username,
            Password = password,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            DispatchConsumersAsync = true,
        };

        Connect();
    }

    private void Connect()
    {
        lock (_lock)
        {
            if (_connection?.IsOpen == true)
                return;

            _logger.LogInformation("Attempting to connect to RabbitMQ at {HostName}:{Port}...", _factory.HostName, _factory.Port);

            _connection = _factory.CreateConnection();

            _connection.ConnectionShutdown += (s, e) =>
                _logger.LogWarning("RabbitMQ connection shutdown: {ReplyText}", e.ReplyText);

            _connection.CallbackException += (s, e) =>
                _logger.LogError(e.Exception, "RabbitMQ callback exception occurred.");

            _connection.ConnectionBlocked += (s, e) =>
                _logger.LogWarning("RabbitMQ connection blocked: {Reason}", e.Reason);

            _logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}", _factory.HostName, _factory.Port);
        }
    }

    public bool IsConnected => _connection?.IsOpen ?? false;

    public IConnection GetConnection()
    {
        if (!IsConnected)
        {
            _logger.LogWarning("RabbitMQ connection lost. Reconnecting...");
            Connect();
        }

        return _connection!;
    }

    public IModel CreateChannel()
    {
        if (!IsConnected)
        {
            _logger.LogWarning("RabbitMQ connection lost. Reconnecting...");
            Connect();
        }

        return _connection!.CreateModel();
    }

    public void Dispose()
    {
        try
        {
            if (_connection?.IsOpen == true)
                _connection.Close();

            _connection?.Dispose();
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMqConnection.");
        }
    }
}