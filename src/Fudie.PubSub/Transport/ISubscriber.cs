namespace Fudie.PubSub.Transport;

public interface ISubscriber
{
    Task SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler, CancellationToken ct = default);
}
