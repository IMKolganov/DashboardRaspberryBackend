using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class PumpSwitcherResponse: IRabbitMqResponse
{
    public Guid RequestId { get; set; }
    public int? PumpId { get; set; }
    public string? Message { get; set; }
    public string Type { get; set; }
}
