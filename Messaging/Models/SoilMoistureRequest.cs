namespace DashboardRaspberryBackend.Messaging.Models;

public class SoilMoistureRequest
{ 
    public Guid RequestId { get; set; }
    public string MethodName { get; set; }
    public int SensorId { get; set; }
    public bool WithoutMSMicrocontrollerManager { get; set; }
    public DateTime CreateDate { get; set; }

}
