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
            { "temperatureResponseQueue", json => JsonConvert.DeserializeObject<TemperatureResponse>(json) },
            { "msgetsoilmoisture.to.backend.response", json => JsonConvert.DeserializeObject<SoilMoistureResponse>(json) },
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