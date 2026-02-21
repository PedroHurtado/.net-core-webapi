namespace Fudie.PubSub.Transport;

public abstract class PubSubClient(ISerializer? serializer = null) : IPubSubClient
{
    private readonly ISerializer _serializer = serializer ?? new JsonPubSubSerializer();

    Task ITopicAdmin.CreateTopicAsync(string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return CreateTopicAsync(topicId);
    }

    Task ITopicAdmin.DeleteTopicAsync(string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return DeleteTopicAsync(topicId);
    }

    Task<bool> ITopicAdmin.TopicExistsAsync(string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return TopicExistsAsync(topicId);
    }

    Task ISubscriptionAdmin.CreateSubscriptionAsync(string subscriptionId, string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return CreateSubscriptionAsync(subscriptionId, topicId);
    }

    Task ISubscriptionAdmin.DeleteSubscriptionAsync(string subscriptionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        return DeleteSubscriptionAsync(subscriptionId);
    }

    Task<bool> ISubscriptionAdmin.SubscriptionExistsAsync(string subscriptionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        return SubscriptionExistsAsync(subscriptionId);
    }

    Task IPublisher.PublishAsync<T>(string topicId, T message)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        ArgumentNullException.ThrowIfNull(message);
        var data = _serializer.Serialize(message);
        return PublishAsync(topicId, data);
    }

    Task ISubscriber.SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeAsync(subscriptionId, (data, token) =>
        {
            var message = _serializer.Deserialize<T>(data);
            return handler(message, token);
        }, ct);
    }

    protected abstract Task CreateTopicAsync(string topicId);
    protected abstract Task DeleteTopicAsync(string topicId);
    protected abstract Task<bool> TopicExistsAsync(string topicId);
    protected abstract Task CreateSubscriptionAsync(string subscriptionId, string topicId);
    protected abstract Task DeleteSubscriptionAsync(string subscriptionId);
    protected abstract Task<bool> SubscriptionExistsAsync(string subscriptionId);
    protected abstract Task PublishAsync(string topicId, byte[] data);
    protected abstract Task SubscribeAsync(string subscriptionId, Func<byte[], CancellationToken, Task> handler, CancellationToken ct);
}
