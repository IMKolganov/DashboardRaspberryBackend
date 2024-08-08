using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConsumer()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Создание очередей при инициализации
        _channel.QueueDeclare(queue: "temperatureQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "temperatureResponseQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task<T> GetMessageAsync<T>(string queueName, string correlationId, int timeoutSeconds)
    {
        var tcs = new TaskCompletionSource<T>();
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var token = cancellationTokenSource.Token;

        token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var response = JsonSerializer.Deserialize<T>(message);

            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                tcs.TrySetResult(response);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        try
        {
            return await tcs.Task;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException($"The operation has timed out after {timeoutSeconds} seconds.");
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
