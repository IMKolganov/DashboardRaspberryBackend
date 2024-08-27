namespace DashboardRaspberryBackend.Messaging.Models;

public class SoilMoistureRequest
{ 
    public Guid RequestId { get; set; }
    public string MethodName { get; set; } = null!;
    public int SensorId { get; set; }
    public bool WithoutMSMicrocontrollerManager { get; set; } = false;
    public DateTime CreateDate { get; set; }

}
