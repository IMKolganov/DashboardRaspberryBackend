using DashboardRaspberryBackend.Messaging.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Messaging;

public class RabbitMqResponseFactory : IRabbitMqResponseFactory
{
    private readonly Dictionary<string, Type> _typeMapping;

    public RabbitMqResponseFactory()
    {
        _typeMapping = new Dictionary<string, Type>
        {
            { "TemperatureHumidityResponse", typeof(GeneralResponse<TemperatureHumidityResponse>) },
            { "SoilMoistureResponse", typeof(GeneralResponse<SoilMoistureResponse>) },
            { "PumpSwitcherResponse", typeof(GeneralResponse<PumpSwitcherResponse>) },
            { "GeneralResponse", typeof(GeneralResponse<IResponse>) }
        };
    }

    public IGeneralResponse<IResponse> CreateModel(string message)
    {
        var jsonObject = JObject.Parse(message);

        var typeName = jsonObject["Data"] is JObject dataObject
            ? dataObject["Type"]?.ToString() ?? "GeneralResponse"
            : "GeneralResponse";

        if (!_typeMapping.TryGetValue(typeName, out var responseType))
        {
            throw new InvalidOperationException($"Unknown type: {typeName}");
        }

        return (IGeneralResponse<IResponse>)JsonConvert.DeserializeObject(message, responseType);
    }
}