using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class PumpResponse: IRabbitMqResponse
{
    public Guid RequestId { get; set; }
    public int? PumpId { get; set; }
    public DateTime CreateDate { get; set; }
    public string? Message { get; set; }
}
