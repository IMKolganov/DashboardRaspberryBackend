using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface IPumpService
{
    Task<GeneralResponse<PumpSwitcherResponse>> StartPum(int pumpId = 0, int pumpDuration = 5, bool useRandomValuesFotTest = false);
}