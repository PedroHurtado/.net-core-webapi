namespace Fudie.PubSub.Gcp.IntegrationTests;

public class SubscriptionTests(GcpFixture fixture) : IClassFixture<GcpFixture>, IAsyncLifetime
{
    private readonly IPubSubClient _client = fixture.Client;
    private readonly List<string> _createdTopics = [];
    private readonly List<string> _createdSubscriptions = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var sub in _createdSubscriptions)
        {
            try { await _client.DeleteSubscriptionAsync(sub); } catch { }
        }
        foreach (var topic in _createdTopics)
        {
            try { await _client.DeleteTopicAsync(topic); } catch { }
        }
    }

    private string UniqueTopic()
    {
        var id = $"topic-{Guid.NewGuid():N}";
        _createdTopics.Add(id);
        return id;
    }

    private string UniqueSub()
    {
        var id = $"sub-{Guid.NewGuid():N}";
        _createdSubscriptions.Add(id);
        return id;
    }

    [Fact]
    public async Task CreateSubscriptionAsync_CreatesSuccessfully()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);

        await _client.CreateSubscriptionAsync(sub, topic);

        var exists = await _client.SubscriptionExistsAsync(sub);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ThrowsWhenDuplicate()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);
        await _client.CreateSubscriptionAsync(sub, topic);

        var act = () => _client.CreateSubscriptionAsync(sub, topic);

        await act.Should().ThrowAsync<RpcException>();
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ThrowsWhenTopicNotExists()
    {
        var sub = UniqueSub();

        var act = () => _client.CreateSubscriptionAsync(sub, $"non-existent-{Guid.NewGuid():N}");

        await act.Should().ThrowAsync<RpcException>();
    }

    [Fact]
    public async Task SubscriptionExistsAsync_ReturnsFalse_WhenNotExists()
    {
        var exists = await _client.SubscriptionExistsAsync($"non-existent-{Guid.NewGuid():N}");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SubscriptionExistsAsync_ReturnsTrue_AfterCreating()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);
        await _client.CreateSubscriptionAsync(sub, topic);

        var exists = await _client.SubscriptionExistsAsync(sub);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_RemovesSubscription()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);
        await _client.CreateSubscriptionAsync(sub, topic);

        await _client.DeleteSubscriptionAsync(sub);

        var exists = await _client.SubscriptionExistsAsync(sub);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_ThrowsWhenNotExists()
    {
        var act = () => _client.DeleteSubscriptionAsync($"non-existent-{Guid.NewGuid():N}");

        await act.Should().ThrowAsync<RpcException>();
    }
}
