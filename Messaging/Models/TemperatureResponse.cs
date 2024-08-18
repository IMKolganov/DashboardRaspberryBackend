using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class TemperatureResponse: IRabbitMqResponse
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}
