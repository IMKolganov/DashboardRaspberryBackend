using DashboardRaspberryBackend.Messaging;
using DashboardRaspberryBackend.Messaging.Models;
using DashboardRaspberryBackend.Services;

public static class ServiceConfiguration
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<RabbitMqProducer>(sp =>
        {
            var requestQueueNames = configuration.GetSection("RabbitMqSettings:RequestQueues").Get<List<string>>();
            return new RabbitMqProducer(requestQueueNames);
        });
        
        services.AddSingleton<RabbitMqConsumer<TemperatureResponse>>(sp =>
        {
            var responseQueueNames = configuration.GetSection("RabbitMqSettings:ResponseQueues").Get<List<string>>();
            return new RabbitMqConsumer<TemperatureResponse>(responseQueueNames);
        });
        
        services.AddScoped<TemperatureService>();
    }
}