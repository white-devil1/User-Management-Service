using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Infrastructure.Services.Messaging;

namespace UserManagementService.Infrastructure.Services.Logging;

public class LogPublisher : ILogPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<LogPublisher> _logger;

    public LogPublisher(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<LogPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void PublishError(ErrorLogEvent evt)
        => FireAndForget("logging.error", evt);

    public void PublishActivity(ActivityLogEvent evt)
        => FireAndForget("logging.activity", evt);

    public void PublishLoginAudit(LoginAuditEvent evt)
        => FireAndForget("logging.loginaudit", evt);

    private void FireAndForget<T>(string exchange, T message) where T : class
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var connection = _connectionFactory.GetConnection();
                await using var channel = await connection.CreateChannelAsync();
                await channel.ExchangeDeclareAsync(
                    exchange, ExchangeType.Fanout,
                    durable: true, autoDelete: false);
                var body = Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(message));
                var props = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                };
                await channel.BasicPublishAsync(
                    exchange, string.Empty, false, props, body);
            }
            catch (Exception ex)
            {
                // Silent fail — only log to Serilog, never affect HTTP response
                _logger.LogWarning(ex,
                    "LogPublisher: failed to publish to {Exchange}",
                    exchange);
            }
        });
    }
}
