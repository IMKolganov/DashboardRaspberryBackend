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

    public async Task<PumpResponse> StartPum(int pumpId = 0, int seconds = 5, bool withoutMSMicrocontrollerManager = false)
    {
        var requestId = Guid.NewGuid();
        _rabbitMqConsumer.RegisterAwaitedMessage(requestId.ToString());
        var request = new PumpRequest
        {
            RequestId = requestId,
            MethodName = "start-pump",
            PumpId = pumpId,
            Seconds = seconds,
            WithoutMSMicrocontrollerManager = withoutMSMicrocontrollerManager,
            CreateDate = DateTime.UtcNow,
        };

        try
        {
            var props = _rabbitMqProducer.CreateBasicProperties();
            props.CorrelationId = requestId.ToString();
            props.ReplyTo = "mspumpcontrol.to.backend.response";
            // Send message in queue
            var timeout = new TimeSpan(0, 0, 30);
            _rabbitMqProducer.SendMessage(request, "backend.to.mspumpcontrol.request", 
                props);
            _logger.LogInformation("Message sent to backend.to.mspumpcontrol.request with " +
                                   "RequestId: {RequestId} , Request: {request}", requestId, request);
            
            var response = (PumpResponse) await _rabbitMqConsumer.GetMessageAsync(requestId.ToString(), timeout);
            _logger.LogInformation("Received response from mspumpcontrol.to.backend.response for " +
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
