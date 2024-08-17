using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqConsumer<T> : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<T>> _pendingRequests = new();
    private readonly List<string> _queueNames;

    public RabbitMqConsumer(List<string> queueNames)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueNames = queueNames ?? new List<string>();

        // Declare and consume all queues
        foreach (var queueName in _queueNames)
        {
            _channel.QueueDeclare(queue: queueName, 
                durable: false, 
                exclusive: false, 
                autoDelete: false,
                arguments: null);

            // Initialize the consumer and attach the Received event handler
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;
            var consumerTag = _channel.BasicConsume(
                queue: queueName, autoAck: false, consumer: consumer);
            Console.WriteLine($"Consumer started with tag: {consumerTag}");
        }
    }

    public async Task<T> GetMessageAsync(string correlationId, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<T>();
        _pendingRequests[correlationId] = tcs;

        var timeoutTask = Task.Delay(timeout).ContinueWith(_ => default(T));
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        _pendingRequests.TryRemove(correlationId, out _);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"The request with correlationId {correlationId} timed out.");
        }

        return await tcs.Task;
    }

    private void OnMessageReceived(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        if (_pendingRequests.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
        {
            try
            {
                // Deserialize the message and complete the task
                var response = JsonConvert.DeserializeObject<T>(message);
                tcs.TrySetResult(response);
            }
            catch (JsonException jsonEx)
            {
                tcs.TrySetException(jsonEx);
            }
            finally
            {
                // Manually acknowledge the message after processing
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }
        else
        {
            // Log a warning or take action for unexpected correlation ID
            Console.WriteLine(
                $"Warning: Received message with unexpected CorrelationId: " +
                $"{ea.BasicProperties.CorrelationId}");

            // Optionally reject the message so it can be requeued or discarded
            //_channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);

            // Acknowledge messages that do not match the correlation ID to prevent re-processing
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
