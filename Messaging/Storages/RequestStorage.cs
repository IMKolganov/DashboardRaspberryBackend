using System.Collections.Concurrent;

namespace DashboardRaspberryBackend.Messaging.Storages;

public class RequestStorage
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _requests = new();

    public void AddRequest(string correlationId, TaskCompletionSource<object> tcs)
    {
        _requests[correlationId] = tcs;
    }

    public bool TryGetRequest(string correlationId, out TaskCompletionSource<object> tcs)
    {
        return _requests.TryRemove(correlationId, out tcs!);
    }
}
