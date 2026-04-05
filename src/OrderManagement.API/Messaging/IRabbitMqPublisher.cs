namespace OrderManagement.API.Messaging;

public interface IRabbitMqPublisher
{
    void Publish<T>(string queueName, T message);
}
