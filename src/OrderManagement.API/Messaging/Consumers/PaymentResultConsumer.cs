using System.Text;
using System.Text.Json;
using MediatR;
using OrderManagement.API.CQRS.Commands;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Messages;

namespace OrderManagement.API.Messaging.Consumers;

public class PaymentResultConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentResultConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentResultConsumer(IServiceScopeFactory scopeFactory, IConfiguration config,
        ILogger<PaymentResultConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        _channel!.QueueDeclare(QueueNames.PaymentResult, durable: true, exclusive: false, autoDelete: false);
        _channel!.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json    = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<PaymentProcessed>(json)!;

                _logger.LogInformation("Received PaymentResult for OrderId={OrderId}, Success={Success}",
                    message.OrderId, message.Success);

                using var scope = _scopeFactory.CreateScope();
                var mediator    = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new ProcessPaymentResultCommand(message));

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment result");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(QueueNames.PaymentResult, autoAck: false, consumer);
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
                _logger.LogInformation("PaymentResultConsumer connected to RabbitMQ");
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
