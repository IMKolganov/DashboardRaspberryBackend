namespace DashboardRaspberryBackend.Messaging.Synchronization;

public enum MessageStatus
{
    Pending,
    Processed,
    TimedOut,
    Failed
}