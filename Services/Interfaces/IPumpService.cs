using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface IPumpService
{
    Task<GeneralResponse<PumpResponse>> StartPum(int pumpId = 0, int seconds = 5, bool withoutMSMicrocontrollerManager = false);
}