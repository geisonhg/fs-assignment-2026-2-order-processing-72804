using System.Text;
using System.Text.Json;
using Inventory.Service.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Messages;

namespace Inventory.Service;

public class Worker(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<Worker> logger)
    : BackgroundService
{
    private IConnection? _connection;
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        _channel!.QueueDeclare(QueueNames.OrderSubmitted,  durable: true, exclusive: false, autoDelete: false);
        _channel!.QueueDeclare(QueueNames.InventoryResult, durable: true, exclusive: false, autoDelete: false);
        _channel!.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;
        _channel.BasicConsume(QueueNames.OrderSubmitted, autoAck: false, consumer);

        logger.LogInformation("Inventory.Service listening on queue '{Queue}'", QueueNames.OrderSubmitted);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        OrderSubmitted? order = null;
        try
        {
            order  = JsonSerializer.Deserialize<OrderSubmitted>(Encoding.UTF8.GetString(ea.Body.ToArray()))!;
            logger.LogInformation("Processing inventory check for OrderId={OrderId}", order.OrderId);

            using var scope = scopeFactory.CreateScope();
            var checker     = scope.ServiceProvider.GetRequiredService<InventoryChecker>();
            var result      = await checker.CheckAsync(order);

            var props = _channel!.CreateBasicProperties();
            props.Persistent = true;
            _channel.BasicPublish("", QueueNames.InventoryResult, props,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));

            _channel.BasicAck(ea.DeliveryTag, false);
            logger.LogInformation("Inventory check complete for OrderId={OrderId}. Success={Success}",
                order.OrderId, result.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing inventory check for OrderId={OrderId}", order?.OrderId);
            _channel!.BasicNack(ea.DeliveryTag, false, requeue: false);
        }
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
            UserName = config["RabbitMQ:Username"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };
        var attempt = 0;
        while (!ct.IsCancellationRequested)
        {
            try { attempt++; _connection = factory.CreateConnection(); _channel = _connection.CreateModel();
                logger.LogInformation("Inventory.Service connected to RabbitMQ"); return; }
            catch (Exception ex)
            { logger.LogWarning("RabbitMQ attempt {Attempt} failed: {Message}. Retrying in 5s...", attempt, ex.Message);
                await Task.Delay(5000, ct); }
        }
    }

    public override void Dispose() { _channel?.Close(); _connection?.Close(); base.Dispose(); }
}
