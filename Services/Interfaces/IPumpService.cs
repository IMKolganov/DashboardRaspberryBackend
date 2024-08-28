using DashboardRaspberryBackend.Messaging.Models;

namespace DashboardRaspberryBackend.Services.Interfaces;

public interface IPumpService
{ 
    Task<PumpResponse> StartPum(int pumpId = 0, bool withoutMSMicrocontrollerManager = false);
}