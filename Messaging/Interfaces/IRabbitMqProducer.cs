using RabbitMQ.Client;

namespace DashboardRaspberryBackend.Messaging.Interfaces;

public interface IRabbitMqProducer
{
    void SendMessage<T>(T message, string queueName,
        IBasicProperties props);
    IBasicProperties CreateBasicProperties();
}