using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ISoilMoistureService
{
    Task<GeneralResponse<SoilMoistureResponse>> GetSoilMoistureData(int sensorId = 0, bool withoutMSMicrocontrollerManager = false);
}