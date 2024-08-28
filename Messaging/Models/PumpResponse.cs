using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class PumpResponse: IRabbitMqResponse
{
    public Guid RequestId { get; set; }
    public string MethodName { get; set; } = null!;
    public int PumpId { get; set; }
    public DateTime CreateDate { get; set; }
    public string? ErrorMessage { get; set; }
}
