using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DashboardRaspberryBackend.Messaging.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqProducer : IRabbitMqProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    public RabbitMqProducer(string hostname, List<string> queueNames)
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            foreach (var queueName in queueNames)
            {
                _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false,
                    arguments: null);
            }
        }
        catch (BrokerUnreachableException ex)
        {
            Console.WriteLine($"RabbitMQ server is unreachable: {ex.Message}");
            throw new InvalidOperationException("BackendError - Could not connect to RabbitMQ server.", ex);
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error: {ex.Message}");
            throw new InvalidOperationException("BackendError - Network error while connecting to RabbitMQ.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw new InvalidOperationException("BackendError - An unexpected error occurred while initializing RabbitMQ producer.", ex);
        }
    }

    public void SendMessage<T>(T message, string queueName,
        IBasicProperties props)
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
