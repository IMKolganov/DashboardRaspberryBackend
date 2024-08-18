using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services;

public class TemperatureService
{
    private readonly RabbitMqProducer _rabbitMqProducer;
    private readonly RabbitMqConsumer<TemperatureResponse> _rabbitMqConsumer;
    private readonly ILogger<TemperatureService> _logger;

    public TemperatureService(RabbitMqProducer rabbitMqProducer, RabbitMqConsumer<TemperatureResponse> rabbitMqConsumer,
        ILogger<TemperatureService> logger)
    {
        _rabbitMqProducer = rabbitMqProducer;
        _rabbitMqConsumer = rabbitMqConsumer;
        _logger = logger;
    }

    public async Task<TemperatureResponse> GetTemperatureAndHumidifyData()
    {
        var requestId = Guid.NewGuid().ToString();
        var request = new TemperatureRequest
        {
            MethodName = "get-temperature-and-humidify",
            IsRandom = true
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId;
            props.ReplyTo = "temperatureResponseQueue";
            // Отправка сообщения в очередь
            _rabbitMqProducer.SendMessage(request, "temperatureRequestQueue", props);
            _logger.LogInformation("Message sent to temperatureRequestQueue with " +
                                   "RequestId: {RequestId}", requestId);

            // Ожидание ответа с таймаутом в 30 секунд
            Console.WriteLine(requestId);
            var response = await _rabbitMqConsumer.GetMessageAsync(requestId, timeout: new TimeSpan(0, 0, 30));
            _logger.LogInformation("Received response from temperatureResponseQueue for " +
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
