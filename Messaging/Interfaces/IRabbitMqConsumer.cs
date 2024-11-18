using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Messaging.Interfaces;

public interface IRabbitMqConsumer
{
    Task<IGeneralResponse<IResponse>> GetMessageAsync(string correlationId, TimeSpan timeout);
    void RegisterAwaitedMessage(string correlationId);
}