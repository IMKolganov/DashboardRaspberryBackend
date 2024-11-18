using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ITemperatureService
{
    Task<IGeneralResponse<TemperatureHumidityResponse?>> GetTemperatureAndHumidifyData(int sensorId = 1,
        bool useRandomValuesFotTest = false);
}