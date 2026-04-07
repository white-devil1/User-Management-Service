using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserManagementService.Application.Services;

namespace UserManagementService.Infrastructure.Services.Messaging;

public class EventPublisher : IEventPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<EventPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var exchangeName = typeof(T).FullName!;
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        try
        {
            var connection = _connectionFactory.GetConnection();

            // Create a channel per publish — channels are lightweight
            // The connection itself stays open (persistent)
            await using var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            // Declare fanout exchange — creates it if it does not exist
            // durable: true = survives RabbitMQ restart
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "RabbitMQ: published {EventType} — MessageId: {MessageId}",
                typeof(T).Name, props.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "RabbitMQ: failed to publish {EventType}", typeof(T).Name);
            throw;
        }
    }
}
