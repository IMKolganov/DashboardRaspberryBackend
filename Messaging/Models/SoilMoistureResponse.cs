using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class SoilMoistureResponse: IRabbitMqResponse
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public int SensorId { get; set; }
    public string Message { get; set; }
    public double? SoilMoistureLevelPercent { get; set; }
    public string Type { get; set; }
}
