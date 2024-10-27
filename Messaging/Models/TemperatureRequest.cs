namespace DashboardRaspberryBackend.Messaging.Models;

public class TemperatureRequest
{ 
    public Guid RequestId { get; set; }
    public string RequestType { get; set; } = "TemperatureHumidity";
    public int SensorId { get; set; }
    public bool WithoutMsMicrocontrollerManager { get; set; }
    public DateTime CreateDate { get; set; }
}
