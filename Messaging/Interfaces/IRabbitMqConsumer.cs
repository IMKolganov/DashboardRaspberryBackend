using DashboardRaspberryBackend.Messaging.Models.Interfaces;

namespace DashboardRaspberryBackend.Messaging.Interfaces;

public interface IRabbitMqConsumer
{
    Task<IRabbitMqResponse> GetMessageAsync(string correlationId, TimeSpan timeout);
    void RegisterAwaitedMessage(string correlationId);
}