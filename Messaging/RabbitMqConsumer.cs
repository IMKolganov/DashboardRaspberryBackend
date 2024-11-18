using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Synchronization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ConcurrentDictionary<string, TaskCompletionSourceWithStatus<IGeneralResponse<IResponse>>> _pendingRequests;
    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _awaitedMessages;
    private readonly IRabbitMqResponseFactory _rabbitMqResponseFactory;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly int _timeoutSeconds;
    private const int CheckIntervalMilliseconds = 200;

    public RabbitMqConsumer(string hostname, int timeoutSeconds, List<string> queueNames, 
        IRabbitMqResponseFactory rabbitMqResponseFactory, ILogger<RabbitMqConsumer> logger)
    {
        _timeoutSeconds = timeoutSeconds;
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _rabbitMqResponseFactory = rabbitMqResponseFactory;
        _logger = logger;
        _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSourceWithStatus<IGeneralResponse<IResponse>>>();
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

    public async Task<IGeneralResponse<IResponse>> GetMessageAsync(string correlationId, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSourceWithStatus<IGeneralResponse<IResponse>>();
        _pendingRequests[correlationId] = tcs;

        _logger.LogInformation("Request with CorrelationId {correlationId} registered.", correlationId);

        using (var cts = new CancellationTokenSource(timeout))
        {
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
            
            if (completedTask == tcs.Task)
            {
                return await tcs.Task; // Successfully received response
            }else {
                _pendingRequests.TryRemove(correlationId, out _);
                tcs.TrySetCanceled();
                _awaitedMessages.TryRemove(correlationId, out _);
                throw new TimeoutException($"The request with correlationId {correlationId} timed out.");
            }
        }
    }

    private void OnMessageReceived(object? model, BasicDeliverEventArgs ea)
    {
        var correlationId = ea.BasicProperties.CorrelationId;
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(_timeoutSeconds))
        {
            if (_awaitedMessages.TryGetValue(correlationId, out var awaitEvent))
            {
                if (_pendingRequests.TryRemove(correlationId, out var tcs2))
                {
                    if (TryToGetMessageFromQueue(tcs2, correlationId, message, ea))
                    {
                        _logger.LogInformation("Message processed for CorrelationId {correlationId} , " +
                                               "Message {message}", correlationId, message);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        awaitEvent.Set();
                        return;
                    }
                }
            }
            Task.Delay(TimeSpan.FromMilliseconds(CheckIntervalMilliseconds)).Wait();
        }

        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
        _logger.LogWarning("Unexpected message with CorrelationId {CorrelationId} , " +
                           "Message {message}", correlationId, message);
    }

    private bool TryToGetMessageFromQueue(TaskCompletionSourceWithStatus<IGeneralResponse<IResponse>> tcs,
        string correlationId, string message, BasicDeliverEventArgs ea)
    {
        try {
            var response = _rabbitMqResponseFactory.CreateModel(message);
            _logger.LogInformation("Response received and deserialized for CorrelationId: {CorrelationId}, Response: {response}", correlationId, response);
            tcs.TrySetResult(response);
            return true;
        } catch (JsonException jsonEx) {
            tcs.TrySetException(jsonEx);
            return false;
        } catch (Exception ex) {
            _logger.LogError(ex, "Unhandled error during message processing.");
            tcs.TrySetException(ex);
            return false;
        }
    }


    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
