namespace DashboardRaspberryBackend.Messaging.Models;

public class PumpRequest
{ 
    public Guid RequestId { get; set; }
    public string MethodName { get; set; } = null!;
    public int PumpId { get; set; }
    public int Seconds { get; set; }
    public bool WithoutMSMicrocontrollerManager { get; set; } = false;
    public DateTime CreateDate { get; set; }

}
