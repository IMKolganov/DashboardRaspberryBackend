using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Services.Interfaces;

namespace DashboardRaspberryBackend.Services;

public class SoilMoistureService : ISoilMoistureService
{
    private readonly IRabbitMqProducer _rabbitMqProducer;
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly ILogger<SoilMoistureService> _logger;

    public SoilMoistureService(IRabbitMqProducer rabbitMqProducer, IRabbitMqConsumer rabbitMqConsumer,
        ILogger<SoilMoistureService> logger)
    {
        _rabbitMqProducer = rabbitMqProducer;
        _rabbitMqConsumer = rabbitMqConsumer;
        _logger = logger;
    }

    public async Task<SoilMoistureResponse> GetSoilMoistureData(int sensorId = 0, bool withoutMSMicrocontrollerManager = false)
    {
        var requestId = Guid.NewGuid();
        var request = new SoilMoistureRequest
        {
            RequestId = requestId,
            MethodName = "get-soil-moisture",
            SensorId = sensorId,
            WithoutMSMicrocontrollerManager = withoutMSMicrocontrollerManager,
            CreateDate = DateTime.UtcNow,
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId.ToString();
            props.ReplyTo = "msgetsoilmoisture.to.backend.response";
            // Send message in queue
            _rabbitMqProducer.SendMessage(request, "backend.to.msgetsoilmoisture.request", props);
            _logger.LogInformation("Message sent to backend.to.msgetsoilmoisture.request with " +
                                   "RequestId: {RequestId}", requestId);//todo: get from settings queue name
            
            Console.WriteLine(requestId);
            var response = (SoilMoistureResponse) await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), 
                new TimeSpan(0, 0, 10));
            _logger.LogInformation("Received response from msgetsoilmoisture.to.backend.response for " +
                                   "RequestId: {RequestId}", requestId);//todo: get from ???
            return response;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout occurred while waiting for soil moisture data.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting soil moisture data.");
            throw;
        }
    }
}
