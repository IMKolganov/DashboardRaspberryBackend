using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Services.Interfaces;
using Newtonsoft.Json;
using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Requests;
using SharedRequests.SmartGarden.Models.Responses;

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

    public async Task<IGeneralResponse<TemperatureHumidityResponse?>> GetTemperatureAndHumidifyData(int sensorId = 1,
        bool useRandomValuesFotTest = false)
    {
        var requestId = Guid.NewGuid();
        _rabbitMqConsumer.RegisterAwaitedMessage(requestId.ToString());

        var temperatureRequest = new TemperatureHumidityRequest
        {
            RequestId = requestId,
            SensorId = sensorId,
            UseRandomValuesFotTest = useRandomValuesFotTest,
            RequestDate = DateTime.UtcNow,
        };

        var generalRequest = new GeneralRequest
        {
            RequestId = requestId,
            RequestType = "TemperatureHumidity",
            Data = temperatureRequest
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId.ToString();
            props.ReplyTo = "msmicrocontrollermanager.to.backend.response";
            var requestQueueName = "backend.to.msmicrocontrollermanager.request";

            // Отправляем сообщение
            var timeout = new TimeSpan(0, 0, 30);
            _rabbitMqProducer.SendMessage(generalRequest, requestQueueName, props);
        
            _logger.LogInformation("Message sent to {requestQueueName} with RequestId: {RequestId}, Request: {request}", 
                requestQueueName, requestId, generalRequest);

            // Ожидание ответа
            var response = await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), timeout);
            _logger.LogInformation("Received response from {props.ReplyTo} for RequestId: {RequestId}, Response: {response}", 
                props.ReplyTo, requestId, JsonConvert.SerializeObject(response));
            
            var result = new GeneralResponse<TemperatureHumidityResponse?>
            {
                RequestId = response.RequestId,
                Success = response?.Success ?? false,
                Message = response?.Message ?? string.Empty,
                ErrorMessage = response?.ErrorMessage ?? string.Empty,
                ResponseDate = response?.ResponseDate ?? DateTime.Now,
                Data = response?.Data != null ? (TemperatureHumidityResponse)response.Data : null
            };

            return result;
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
