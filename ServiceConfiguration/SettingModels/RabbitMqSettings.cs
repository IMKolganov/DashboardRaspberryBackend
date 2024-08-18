namespace DashboardRaspberryBackend.ServiceConfiguration.SettingModels;

public class RabbitMqSettings
{
    public string HostName { get; set; }
    public List<string> RequestQueues { get; set; }
    public List<string> ResponseQueues { get; set; }
}