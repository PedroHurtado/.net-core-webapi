namespace Fudie.PubSub;

public interface IPubSubClient
{
    Task CreateTopicAsync(string topicId);
    Task DeleteTopicAsync(string topicId);
    Task<bool> TopicExistsAsync(string topicId);
    Task CreateSubscriptionAsync(string subscriptionId, string topicId);
    Task DeleteSubscriptionAsync(string subscriptionId);
    Task<bool> SubscriptionExistsAsync(string subscriptionId);
    Task PublishAsync(string topicId, byte[] data);
    Task SubscribeAsync(string subscriptionId, Func<byte[], CancellationToken, Task> handler, CancellationToken ct = default);
}
