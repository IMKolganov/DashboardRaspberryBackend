using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConsumer()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task<T> GetMessageAsync<T>(string queueName, string correlationId)
    {
        var tcs = new TaskCompletionSource<T>();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var response = JsonSerializer.Deserialize<T>(message);

            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                tcs.SetResult(response);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        return tcs.Task;
    }
}
