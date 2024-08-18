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

    public async Task<SoilMoistureResponse> GetSoilMoistureData()
    {
        var requestId = Guid.NewGuid().ToString();
        var request = new SoilMoistureRequest
        {
            MethodName = "get-soil-moisture",
            IsRandom = true
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId;
            props.ReplyTo = "soilMoistureResponseQueue";
            // Send message in queue
            _rabbitMqProducer.SendMessage(request, "soilMoistureRequestQueue", props);
            _logger.LogInformation("Message sent to soilMoistureRequestQueue with " +
                                   "RequestId: {RequestId}", requestId);
            
            Console.WriteLine(requestId);
            var response = (SoilMoistureResponse) await _rabbitMqConsumer.GetMessageAsync(requestId, 
                new TimeSpan(0, 0, 5));
            _logger.LogInformation("Received response from soilMoistureResponseQueue for " +
                                   "RequestId: {RequestId}", requestId);
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
