namespace DashboardRaspberryBackend.Messaging.Synchronization;

public class TaskCompletionSourceWithStatus<T> : TaskCompletionSource<T>
{
    public MessageStatus Status { get; private set; } = MessageStatus.Pending;

    public new void SetResult(T result)
    {
        Status = MessageStatus.Processed;
        base.SetResult(result);
    }

    public new void SetException(Exception exception)
    {
        Status = MessageStatus.Failed;
        base.SetException(exception);
    }

    public new void SetCanceled()
    {
        Status = MessageStatus.TimedOut;
        base.SetCanceled();
    }
}