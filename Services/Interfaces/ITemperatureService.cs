using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ITemperatureService
{
    Task<GeneralResponse<TemperatureResponse>> GetTemperatureAndHumidifyData(bool withoutMSMicrocontrollerManager = false);
}