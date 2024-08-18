using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Interfaces;

public interface IRabbitMqResponseFactory
{
    IRabbitMqResponse CreateModel(string json, string modelType);
}