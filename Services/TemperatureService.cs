using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Services.Interfaces;

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
            props.ReplyTo = "msgettemperatureandhumidify.to.backend.response";
            // Send message in queue
            _rabbitMqProducer.SendMessage(request, "backend.to.msgettemperatureandhumidify.request", props);
            _logger.LogInformation("Message sent to backend.to.msgettemperatureandhumidify.request with " +
                                   "RequestId: {RequestId}", requestId);
            
            Console.WriteLine(requestId);
            var response = (TemperatureResponse) await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), 
                new TimeSpan(0, 0, 5));
            _logger.LogInformation("Received response from msgettemperatureandhumidify.to.backend.response for " +
                                   "RequestId: {RequestId}", requestId);
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
