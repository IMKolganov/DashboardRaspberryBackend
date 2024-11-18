using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ITemperatureService
{
    Task<GeneralResponse<TemperatureHumidityResponse>> GetTemperatureAndHumidifyData(int sensorId = 1, bool useRandomValuesFotTest = false);
}