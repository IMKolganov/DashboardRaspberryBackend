using SharedRequests.SmartGarden.Models;
using SharedRequests.SmartGarden.Models.Responses;

namespace DashboardRaspberryBackend.Messaging.Interfaces;

public interface IRabbitMqResponseFactory
{
    IGeneralResponse<IResponse> CreateModel(string json);
}