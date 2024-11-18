using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Messaging.Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqResponseFactory : IRabbitMqResponseFactory
{
    private readonly Dictionary<string, Type> _typeMapping;

    public RabbitMqResponseFactory()
    {
        _typeMapping = new Dictionary<string, Type>
        {
            { "TemperatureHumidityResponse", typeof(TemperatureHumidityResponse) },
            { "SoilMoistureResponse", typeof(SoilMoistureResponse) },
            { "PumpSwitcherResponse", typeof(PumpSwitcherResponse) }
        };
    }

    public IRabbitMqResponse CreateModel(string message)
    {
        var jsonObject = JObject.Parse(message);

        var typeName = jsonObject["Type"]?.ToString() ?? "Unknown";
        if (!_typeMapping.TryGetValue(typeName, out var responseType))
        {
            throw new InvalidOperationException($"Unknown type: {typeName}");
        }

        return (IRabbitMqResponse)JsonConvert.DeserializeObject(message, responseType);
    }
}