using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.ServiceConfiguration.SettingModels;
using DashboardRaspberryBackend.Services;

namespace DashboardRaspberryBackend.ServiceConfiguration;

public static class RabbitMqConfiguration
{
    public static void AddRabbitMqServices(this IServiceCollection services, 
        IConfiguration configuration)
    {
        // services.AddSingleton<RequestStorage>();//todo: please looking for todo in RabbitMqConsumer
        var rabbitMqSettings = new RabbitMqSettings();
        configuration.GetSection("RabbitMqSettings").Bind(rabbitMqSettings);
        
        services.AddSingleton(rabbitMqSettings);
        services.AddSingleton<RabbitMqProducer>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMqSettings>();
            return new RabbitMqProducer(settings.HostName, settings.RequestQueues);
        });
        
        services.AddSingleton<RabbitMqConsumer<TemperatureResponse>>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMqSettings>();
            return new RabbitMqConsumer<TemperatureResponse>(settings.HostName, settings.ResponseQueues);
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