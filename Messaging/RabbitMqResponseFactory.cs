using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Messaging.Models.Interfaces;
using Newtonsoft.Json;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqResponseFactory : IRabbitMqResponseFactory
{
    private readonly IDictionary<string, Func<string, IRabbitMqResponse>> _modelCreators;

    public RabbitMqResponseFactory()
    {
        _modelCreators = new Dictionary<string, Func<string, IRabbitMqResponse>>
        {
            { "msgettemperatureandhumidify.to.backend.response", json => JsonConvert.DeserializeObject<TemperatureResponse>(json) ?? throw new InvalidOperationException() },
            { "msgetsoilmoisture.to.backend.response", json => JsonConvert.DeserializeObject<SoilMoistureResponse>(json) ?? throw new InvalidOperationException() },
            { "mspumpcontrol.to.backend.response", json => JsonConvert.DeserializeObject<PumpResponse>(json) ?? throw new InvalidOperationException() },
        };
    }
    
    public IRabbitMqResponse CreateModel(string json, string modelType)
    {
        if (_modelCreators.TryGetValue(modelType, out var createModel))
        {
            return createModel(json);
        }

        throw new ArgumentException($"Model type {modelType} is not supported.", nameof(modelType));
    }
}