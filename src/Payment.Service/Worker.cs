using System.Text;
using System.Text.Json;
using Payment.Service.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Messages;

namespace Payment.Service;

public class Worker(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<Worker> logger)
    : BackgroundService
{
    private IConnection? _connection;
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        _channel!.QueueDeclare(QueueNames.PaymentRequested, durable: true, exclusive: false, autoDelete: false);
        _channel!.QueueDeclare(QueueNames.PaymentResult,    durable: true, exclusive: false, autoDelete: false);
        _channel!.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;
        _channel.BasicConsume(QueueNames.PaymentRequested, autoAck: false, consumer);

        logger.LogInformation("Payment.Service listening on queue '{Queue}'", QueueNames.PaymentRequested);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        OrderSubmitted? order = null;
        try
        {
            order = JsonSerializer.Deserialize<OrderSubmitted>(Encoding.UTF8.GetString(ea.Body.ToArray()))!;
            logger.LogInformation("Processing payment for OrderId={OrderId}, Amount={Amount:C}", order.OrderId, order.TotalAmount);

            using var scope = scopeFactory.CreateScope();
            var result      = scope.ServiceProvider.GetRequiredService<PaymentProcessor>().Process(order);

            var props = _channel!.CreateBasicProperties(); props.Persistent = true;
            _channel.BasicPublish("", QueueNames.PaymentResult, props, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));
            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment for OrderId={OrderId}", order?.OrderId);
            _channel!.BasicNack(ea.DeliveryTag, false, requeue: false);
        }
        await Task.CompletedTask;
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost", Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
            UserName = config["RabbitMQ:Username"] ?? "guest", Password = config["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };
        var attempt = 0;
        while (!ct.IsCancellationRequested)
        {
            try { attempt++; _connection = factory.CreateConnection(); _channel = _connection.CreateModel();
                logger.LogInformation("Payment.Service connected to RabbitMQ"); return; }
            catch (Exception ex)
            { logger.LogWarning("RabbitMQ attempt {Attempt} failed: {Message}. Retrying in 5s...", attempt, ex.Message);
                await Task.Delay(5000, ct); }
        }
    }

    public override void Dispose() { _channel?.Close(); _connection?.Close(); base.Dispose(); }
}
