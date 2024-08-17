namespace DashboardRaspberryBackend.Messaging.Models;

public class RequestState
{
    public TaskCompletionSource<object> TaskCompletionSource { get; set; }
    public DateTime RequestTime { get; set; }
}