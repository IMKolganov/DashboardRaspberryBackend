using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqProducer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public RabbitMqProducer(string hostname, List<string> queueNames)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare all queues to ensure they exist
        foreach (var queueName in queueNames)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false,
                arguments: null);
        }
    }

    public void SendMessage<T>(T message, string queueName, IBasicProperties props)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange: string.Empty,
            routingKey: queueName,
            basicProperties: props,
            body: body);
    }

    public IBasicProperties CreateBasicProperties()
    {
        return _channel.CreateBasicProperties();
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}