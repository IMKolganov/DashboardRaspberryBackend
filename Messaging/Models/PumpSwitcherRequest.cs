using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class PumpSwitcherRequest : IRequest
{
    public PumpSwitcherRequest()
    {
    }
    public PumpSwitcherRequest(Guid requestId)
    {
        RequestId = requestId;
    }
    public Guid RequestId { get; set; }
    public bool UseRandomValuesFotTest { get; set; }
    public DateTime RequestDate { get; set; }
    public int PumpId { get; set; }
    public int Duration { get; set; }
}