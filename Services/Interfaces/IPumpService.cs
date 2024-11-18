using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface IPumpService
{
    Task<GeneralResponse<IResponse?>> StartPum(int pumpId = 0, int pumpDuration = 5, bool useRandomValuesFotTest = false);
}