namespace Fudie.PubSub.UnitTests;

internal sealed class StubPubSubClient(ISerializer? serializer = null) : PubSubClient(serializer)
{
    public byte[]? LastPublishedData { get; private set; }
    public byte[]? DataToDeliver { get; init; }

    protected override Task CreateTopicAsync(string topicId) => Task.CompletedTask;
    protected override Task DeleteTopicAsync(string topicId) => Task.CompletedTask;
    protected override Task<bool> TopicExistsAsync(string topicId) => Task.FromResult(false);
    protected override Task CreateSubscriptionAsync(string subscriptionId, string topicId) => Task.CompletedTask;
    protected override Task DeleteSubscriptionAsync(string subscriptionId) => Task.CompletedTask;
    protected override Task<bool> SubscriptionExistsAsync(string subscriptionId) => Task.FromResult(false);

    protected override Task PublishAsync(string topicId, byte[] data)
    {
        LastPublishedData = data;
        return Task.CompletedTask;
    }

    protected override async Task SubscribeAsync(string subscriptionId, Func<byte[], CancellationToken, Task> handler, CancellationToken ct)
    {
        if (DataToDeliver is not null)
            await handler(DataToDeliver, ct);
    }
}
