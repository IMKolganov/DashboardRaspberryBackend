using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Models;

public class GeneralResponse<T> : IRabbitMqResponse
{
    public GeneralResponse()
    {
        Type = nameof(GeneralResponse<T>);
    }
    
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public T Data { get; set; }
    public string Type { get; set; }
}