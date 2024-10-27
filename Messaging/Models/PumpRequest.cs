namespace DashboardRaspberryBackend.Messaging.Models;

public class PumpRequest
{ 
    public Guid RequestId { get; set; }
    public string RequestType { get; set; } = "PumpSwitcher";
    public int PumpId { get; set; }
    public int Seconds { get; set; }
    public bool WithoutMsMicrocontrollerManager { get; set; } = false;
    public DateTime CreateDate { get; set; }

}
