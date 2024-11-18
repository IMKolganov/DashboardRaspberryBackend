using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface ISoilMoistureService
{
    Task<GeneralResponse<SoilMoistureResponse?>> GetSoilMoistureData(int sensorId = 0, bool useRandomValuesFotTest = false);
}