namespace DashboardRaspberryBackend.Messaging.Models;

public class TemperatureRequest
{ 
    public Guid RequestId { get; set; }
    public string MethodName { get; set; }
    public bool WithoutMSMicrocontrollerManager { get; set; }
    public DateTime CreateDate { get; set; }
}
