using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;

namespace UserManagementService.Infrastructure.Services.Messaging;

public class RabbitMqConnectionFactory : IDisposable
{
    private IConnection? _connection;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;
    private readonly ResiliencePipeline _connectPipeline;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqConnectionFactory(
        IConfiguration configuration,
        ILogger<RabbitMqConnectionFactory> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _logger = logger;
        _connectPipeline = pipelineProvider.GetPipeline("rabbitmq-connect");
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    public IConnection GetConnection()
    {
        if (_connection != null && _connection.IsOpen)
            return _connection;

        lock (_lock)
        {
            if (_connection != null && _connection.IsOpen)
                return _connection;

            _logger.LogInformation(
                "RabbitMQ: creating persistent connection to {Host}", _factory.HostName);

            _connection = _connectPipeline.ExecuteAsync(async ct =>
                await _factory.CreateConnectionAsync(ct)).GetAwaiter().GetResult();

            _logger.LogInformation("RabbitMQ: connection established successfully");
            return _connection;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ: connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ: error closing connection");
        }
    }
}
