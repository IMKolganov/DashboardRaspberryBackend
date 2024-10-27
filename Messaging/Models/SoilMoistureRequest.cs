namespace DashboardRaspberryBackend.Messaging.Models;

public class SoilMoistureRequest
{ 
    public Guid RequestId { get; set; }
    public string RequestType { get; set; } = "SoilMoisture";
    public int SensorId { get; set; }
    public bool WithoutMSMicrocontrollerManager { get; set; } = false;
    public DateTime CreateDate { get; set; }

}
