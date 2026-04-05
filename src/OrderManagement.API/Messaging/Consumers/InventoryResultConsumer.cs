using System.Text;
using System.Text.Json;
using MediatR;
using OrderManagement.API.CQRS.Commands;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Messages;

namespace OrderManagement.API.Messaging.Consumers;

public class InventoryResultConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<InventoryResultConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public InventoryResultConsumer(IServiceScopeFactory scopeFactory, IConfiguration config,
        ILogger<InventoryResultConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        _channel!.QueueDeclare(QueueNames.InventoryResult, durable: true, exclusive: false, autoDelete: false);
        _channel!.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json    = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<InventoryCheckCompleted>(json)!;

                _logger.LogInformation("Received InventoryResult for OrderId={OrderId}, Success={Success}",
                    message.OrderId, message.Success);

                using var scope = _scopeFactory.CreateScope();
                var mediator    = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ProcessInventoryResultCommand(message));

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory result");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(QueueNames.InventoryResult, autoAck: false, consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
            UserName = _config["RabbitMQ:Username"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };
        var attempt = 0;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                attempt++;
                _connection = factory.CreateConnection();
                _channel    = _connection.CreateModel();
                _logger.LogInformation("InventoryResultConsumer connected to RabbitMQ");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ connection attempt {Attempt} failed: {Message}. Retrying in 5s...", attempt, ex.Message);
                await Task.Delay(5000, ct);
            }
        }
    }

    public override void Dispose() { _channel?.Close(); _connection?.Close(); base.Dispose(); }
}
