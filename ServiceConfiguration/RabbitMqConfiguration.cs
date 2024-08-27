using System.Collections.Concurrent;
using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Interfaces;
using DashboardRaspberryBackend.Messaging.Models.Interfaces;
using DashboardRaspberryBackend.ServiceConfiguration.SettingModels;
using DashboardRaspberryBackend.Services;
using DashboardRaspberryBackend.Services.Interfaces;

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
        services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMqSettings>();
            return new RabbitMqProducer(settings.HostName, settings.RequestQueues);
        });
        
        services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>(sp =>
        {
            var settings = sp.GetRequiredService<RabbitMqSettings>();
            var rabbitMqResponseFactory = new RabbitMqResponseFactory();
            var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer>>();
            return new RabbitMqConsumer(settings.HostName, settings.TimeoutSeconds, settings.ResponseQueues,
                rabbitMqResponseFactory, logger);
        });
        
        services.AddScoped<ITemperatureService, TemperatureService>();
        services.AddScoped<ISoilMoistureService, SoilMoistureService>();
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