using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqProducer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqProducer()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Создание очередей при инициализации
        _channel.QueueDeclare(queue: "temperatureQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "temperatureResponseQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void SendMessage<T>(T message, string queueName)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
