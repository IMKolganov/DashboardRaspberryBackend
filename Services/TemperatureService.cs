using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Services.Interfaces;
using Newtonsoft.Json;

namespace DashboardRaspberryBackend.Services;

public class TemperatureService : ITemperatureService
{
    private readonly IRabbitMqProducer _rabbitMqProducer;
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly ILogger<TemperatureService> _logger;

    public TemperatureService(IRabbitMqProducer rabbitMqProducer, IRabbitMqConsumer rabbitMqConsumer,
        ILogger<TemperatureService> logger)
    {
        _rabbitMqProducer = rabbitMqProducer;
        _rabbitMqConsumer = rabbitMqConsumer;
        _logger = logger;
    }

    public async Task<TemperatureResponse> GetTemperatureAndHumidifyData(bool withoutMSMicrocontrollerManager = false)
    {
        var requestId = Guid.NewGuid();
        _rabbitMqConsumer.RegisterAwaitedMessage(requestId.ToString());
        var request = new TemperatureRequest
        {
            RequestId = requestId,
            MethodName = "get-temperature-and-humidify",
            
            WithoutMSMicrocontrollerManager = withoutMSMicrocontrollerManager,
            CreateDate = DateTime.UtcNow,
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId.ToString();
            var requestQueueName = "backend.to.msgettemperatureandhumidify.request";
            props.ReplyTo = "msgettemperatureandhumidify.to.backend.response";
            // Send message in queue
            var timeout = new TimeSpan(0, 0, 10);
            _rabbitMqProducer.SendMessage(request,
                requestQueueName, props);
            _logger.LogInformation("Message sent to {requestQueueName} with " +
                                   "RequestId: {RequestId} , Request: {request}", 
                requestQueueName, requestId, request);
            
            var response = (TemperatureResponse) await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), timeout);
            _logger.LogInformation("Received response from {props.ReplyTo} for " +
                                   "RequestId: {RequestId} , Response: {response}", 
                props.ReplyTo, requestId,  JsonConvert.SerializeObject(response));
            return response;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout occurred while waiting for temperature and humidity data.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting temperature and humidity data.");
            throw;
        }
    }
}
