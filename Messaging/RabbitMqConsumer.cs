using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models.Interfaces;
using DashboardRaspberryBackend.Messaging.Synchronization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ConcurrentDictionary<string, TaskCompletionSourceWithStatus<IRabbitMqResponse>> _pendingRequests;
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _awaitedMessages;
    private readonly IRabbitMqResponseFactory _rabbitMqResponseFactory;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(string hostname, List<string> queueNames, 
        IRabbitMqResponseFactory rabbitMqResponseFactory, ILogger<RabbitMqConsumer> logger)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _rabbitMqResponseFactory = rabbitMqResponseFactory;
        _logger = logger;
        _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSourceWithStatus<IRabbitMqResponse>>();
        _awaitedMessages = new ConcurrentDictionary<string, ManualResetEventSlim>();

        foreach (var queueName in queueNames)
        {
            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;
            var consumerTag = _channel.BasicConsume(
                queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Consumer started with tag: {consumerTag}", consumerTag);
        }
    }
    
    public void RegisterAwaitedMessage(string correlationId)
    {
        var awaitEvent = new ManualResetEventSlim(false);
        _awaitedMessages[correlationId] = awaitEvent;
    }

    public async Task<IRabbitMqResponse> GetMessageAsync(string correlationId, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSourceWithStatus<IRabbitMqResponse>();
        _pendingRequests[correlationId] = tcs;

        _logger.LogInformation("Request with CorrelationId {correlationId} registered.", correlationId);

        using (var cts = new CancellationTokenSource(timeout))
        {
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
            
            if (completedTask == tcs.Task)
            {
                // Successfully received response
                return await tcs.Task;
            }
            else
            {
                // Timeout occurred
                _pendingRequests.TryRemove(correlationId, out _);
                throw new TimeoutException($"The request with correlationId {correlationId} timed out.");
            }
        }
    }

    private void OnMessageReceived(object? model, BasicDeliverEventArgs ea)
    {
        var correlationId = ea.BasicProperties.CorrelationId;
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var messageFound = false;

        if (_pendingRequests.TryRemove(correlationId, out var tcs))
        {
            messageFound = TryToGetMessageFromQueue(tcs, correlationId, message, ea);
        } else {
            var checkInterval = TimeSpan.FromMilliseconds(500);  // Интервал между проверками
            var timeout = TimeSpan.FromSeconds(5);               // Общий таймаут ожидания
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                if (_awaitedMessages.TryGetValue(correlationId, out var awaitEvent))
                {
                    var b = _pendingRequests.TryGetValue(correlationId, 
                        out var value);
                    if (_pendingRequests.TryRemove(correlationId, out var tcs2))
                    {
                        _logger.LogError("Сообщение найдено, но его нет " +
                                         " b {b} " +
                                         "в _pendingRequests {_pendingRequests}", 
                            b, _pendingRequests);
                        messageFound = TryToGetMessageFromQueue(tcs2, correlationId, message, ea); ;
                        awaitEvent.Set();
                        break;
                    }
                }
                Task.Delay(checkInterval);  // Ждем перед следующей проверкой
            }
        }
        
        if (!messageFound)
        {
            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            _logger.LogWarning("Unexpected message with CorrelationId {CorrelationId}", correlationId);
        }

    }

    private bool TryToGetMessageFromQueue(TaskCompletionSource<IRabbitMqResponse>? tcs,
        string correlationId, string message, BasicDeliverEventArgs ea)
    {
        try
        {
            var response = _rabbitMqResponseFactory.CreateModel(message, ea.RoutingKey);
            _logger.LogInformation("Response received and deserialized for CorrelationId: {CorrelationId} , " +
                                   "Response: {response}", correlationId, response);
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            _logger.LogInformation("BasicAck Manually {DeliveryTag}", ea.DeliveryTag);
            tcs.TrySetResult(response);
            return true;
        }
        catch (JsonException jsonEx)
        {
            tcs.TrySetException(jsonEx);
        }
        // finally
        // {
        // _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        // _logger.LogInformation("BasicAck Manually {DeliveryTag}", ea.DeliveryTag);
        // }

        return false;
    }


    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
