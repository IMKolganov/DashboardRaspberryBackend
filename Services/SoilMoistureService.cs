using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Services.Interfaces;
using Newtonsoft.Json;
using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Requests;
using SharedRequests.SmartGarden.Models.Responses;

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

    public async Task<GeneralResponse<SoilMoistureResponse?>> GetSoilMoistureData(int sensorId = 0, bool useRandomValuesFotTest = false)
    {
        var requestId = Guid.NewGuid();
        _rabbitMqConsumer.RegisterAwaitedMessage(requestId.ToString());
        var soilMoistureRequest = new SoilMoistureRequest
        {
            RequestId = requestId,
            SensorId = sensorId,
            UseRandomValuesFotTest = useRandomValuesFotTest,
            RequestDate = DateTime.UtcNow,
        };
        
        var generalRequest = new GeneralRequest
        {
            RequestId = requestId,
            RequestType = "SoilMoisture",
            Data = soilMoistureRequest
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId.ToString();
            props.ReplyTo = "msmicrocontrollermanager.to.backend.response";
            // Send message in queue
            var timeout = new TimeSpan(0, 0, 30);
            _rabbitMqProducer.SendMessage(generalRequest, "backend.to.msmicrocontrollermanager.request", 
                props);
            _logger.LogInformation("Message sent to backend.to.msmicrocontrollermanager.request with " +
                                   "RequestId: {RequestId} , Request: {generalRequest}", requestId, generalRequest);
            
            var response = await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), timeout);
            _logger.LogInformation("Received response from msmicrocontrollermanager.to.backend.response for " +
                                   "RequestId: {RequestId} , Response: {response}", 
                requestId,  JsonConvert.SerializeObject(response));
            
            return new GeneralResponse<SoilMoistureResponse?>
            {
                RequestId = response.RequestId,
                Success = response?.Success ?? false,
                Message = response?.Message ?? string.Empty,
                ErrorMessage = response?.ErrorMessage ?? string.Empty,
                ResponseDate = response?.ResponseDate ?? DateTime.Now,
                Data = response?.Data != null ? (SoilMoistureResponse)response.Data : null
            };
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
