using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ITemperatureService
{
    Task<TemperatureResponse> GetTemperatureAndHumidifyData(bool withoutMSMicrocontrollerManager = false);
}