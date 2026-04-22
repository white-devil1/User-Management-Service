using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using UserManagementService.Application.Services;

namespace UserManagementService.Infrastructure.Services.Messaging;

public class EventPublisher : IEventPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<EventPublisher> _logger;
    private readonly ResiliencePipeline _pipeline;

    public EventPublisher(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<EventPublisher> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _pipeline = pipelineProvider.GetPipeline("rabbitmq-publish");
    }

    public async Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var exchangeName = typeof(T).FullName!;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        try
        {
            await _pipeline.ExecuteAsync(async ct =>
            {
                var connection = _connectionFactory.GetConnection();
                await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

                await channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: ct);

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
                    cancellationToken: ct);

                _logger.LogInformation(
                    "RabbitMQ: published {EventType} — MessageId: {MessageId}",
                    typeof(T).Name, props.MessageId);

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "RabbitMQ: failed to publish {EventType} after retries", typeof(T).Name);
            throw;
        }
    }
}
