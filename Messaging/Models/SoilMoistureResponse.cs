using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class SoilMoistureResponse: IRabbitMqResponse
{
    public double SoilMoistureLevel { get; set; }
}
