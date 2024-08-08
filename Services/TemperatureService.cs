using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services;

public class TemperatureService
{
    private readonly RabbitMqProducer _rabbitMqProducer;
    private readonly RabbitMqConsumer _rabbitMqConsumer;
    private readonly ILogger<TemperatureService> _logger;

    public TemperatureService(RabbitMqProducer rabbitMqProducer, RabbitMqConsumer rabbitMqConsumer, ILogger<TemperatureService> logger)
    {
        _rabbitMqProducer = rabbitMqProducer;
        _rabbitMqConsumer = rabbitMqConsumer;
        _logger = logger;
    }

    public async Task<string> GetTemperatureAndHumidifyData()
    {
        var requestId = Guid.NewGuid().ToString();
        var request = new TemperatureRequest
        {
            RequestId = requestId,
            Url = "/get-temperature-and-humidify"
        };

        try
        {
            // Отправка сообщения в очередь
            _rabbitMqProducer.SendMessage(request, "temperatureQueue");
            _logger.LogInformation("Message sent to temperatureQueue with RequestId: {RequestId}", requestId);

            // Ожидание ответа с таймаутом в 30 секунд
            var response = await _rabbitMqConsumer.GetMessageAsync<TemperatureResponse>("temperatureResponseQueue", requestId, 30);
            _logger.LogInformation("Received response from temperatureResponseQueue for RequestId: {RequestId}", requestId);

            return response.Data;
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
