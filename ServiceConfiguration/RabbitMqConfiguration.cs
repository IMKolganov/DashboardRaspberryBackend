using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Services;

namespace DashboardRaspberryBackend.ServiceConfiguration;

public static class RabbitMqConfiguration
{
    public static void AddRabbitMqServices(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddSingleton<RabbitMqProducer>(sp =>
        {
            var requestQueueNames = configuration.GetSection(
                "RabbitMqSettings:RequestQueues").Get<List<string>>();
            return new RabbitMqProducer(requestQueueNames);
        });
        
        services.AddSingleton<RabbitMqConsumer<TemperatureResponse>>(sp =>
        {
            var responseQueueNames = configuration.GetSection(
                "RabbitMqSettings:ResponseQueues").Get<List<string>>();
            return new RabbitMqConsumer<TemperatureResponse>(responseQueueNames);
        });
        
        services.AddScoped<TemperatureService>();
    }

    public static void AddHttpClientsForRabbitMq(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient("TemperatureAndHumidifyService", client =>
        {
            client.BaseAddress = new Uri(
                configuration["MicroserviceSettings:GetTemperatureAndHumidifyUrl"]);
        });
        services.AddHttpClient("SoilMoistureService", client =>
        {
            client.BaseAddress = new Uri(
                configuration["MicroserviceSettings:GetSoilMoistureUrl"]);
        });
    }
}