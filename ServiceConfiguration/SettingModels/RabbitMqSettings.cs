namespace DashboardRaspberryBackend.ServiceConfiguration.SettingModels;

public class RabbitMqSettings
{
    public string HostName { get; set; } = null!;
    public int TimeoutSeconds { get; set; } = 30;
    public List<string> RequestQueues { get; set; } = null!;
    public List<string> ResponseQueues { get; set; } = null!;
}