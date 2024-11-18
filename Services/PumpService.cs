using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Services.Interfaces;
using Newtonsoft.Json;

namespace DashboardRaspberryBackend.Services;

public class PumpService : IPumpService
{
    private readonly IRabbitMqProducer _rabbitMqProducer;
    private readonly IRabbitMqConsumer _rabbitMqConsumer;
    private readonly ILogger<PumpService> _logger;

    public PumpService(IRabbitMqProducer rabbitMqProducer, IRabbitMqConsumer rabbitMqConsumer,
        ILogger<PumpService> logger)
    {
        _rabbitMqProducer = rabbitMqProducer;
        _rabbitMqConsumer = rabbitMqConsumer;
        _logger = logger;
    }

    public async Task<GeneralResponse<PumpSwitcherResponse>> StartPum(int pumpId = 0, int pumpDuration = 5, bool useRandomValuesFotTest = false)
    {
        var requestId = Guid.NewGuid();
        _rabbitMqConsumer.RegisterAwaitedMessage(requestId.ToString());
        var pumpRequest = new PumpSwitcherRequest
        {
            RequestId = requestId,
            PumpId = pumpId,
            Duration = pumpDuration,
            UseRandomValuesFotTest = useRandomValuesFotTest,
            RequestDate = DateTime.UtcNow,
        };
        
        var generalRequest = new GeneralRequest
        {
            RequestId = requestId,
            RequestType = "PumpSwitcher",
            Data = pumpRequest
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
            
            var response = (GeneralResponse<PumpSwitcherResponse>) await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), timeout);
            _logger.LogInformation("Received response from msmicrocontrollermanager.to.backend.response for " +
                                   "RequestId: {RequestId} , Response: {response}", 
                requestId,  JsonConvert.SerializeObject(response));
            return response;
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout occurred while waiting for start pump.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while in start pump.");
            throw;
        }
    }
}
