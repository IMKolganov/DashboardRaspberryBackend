using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class SoilMoistureResponse: IRabbitMqResponse
{
    public Guid RequestId { get; set; }
    public string MethodName { get; set; } = null!;
    public int SensorId { get; set; }
    public double SoilMoistureLevelPercent { get; set; }
    public DateTime CreateDate { get; set; }
    public string? ErrorMessage { get; set; }
}
