using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ISoilMoistureService
{
    Task<SoilMoistureResponse> GetSoilMoistureData();
}