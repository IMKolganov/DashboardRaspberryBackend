using System;
using System.Threading.Tasks;
using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;
using Microsoft.Extensions.Logging;

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

            // Ожидание ответа
            var response = await _rabbitMqConsumer.GetMessageAsync<TemperatureResponse>("temperatureResponseQueue", requestId);
            _logger.LogInformation("Received response from temperatureResponseQueue for RequestId: {RequestId}", requestId);

            return response.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving temperature and humidity data.");
            throw;
        }
    }
}
