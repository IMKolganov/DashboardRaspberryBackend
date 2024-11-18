namespace DashboardRaspberryBackend.Messaging.Models;

public class GeneralRequest
{
    public Guid RequestId { get; set; }
    public string RequestType { get; set; }
    public object Data { get; set; }
}