namespace DashboardRaspberryBackend.Messaging.Models;

public class RequestState
{
    public TaskCompletionSource<object> TaskCompletionSource { get; set; } = null!;
    public DateTime RequestTime { get; set; }
}