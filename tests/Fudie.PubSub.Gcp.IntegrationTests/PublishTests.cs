namespace Fudie.PubSub.Gcp.IntegrationTests;

public record TestMessage(string Text);

public class PublishTests(GcpFixture fixture) : IClassFixture<GcpFixture>, IAsyncLifetime
{
    private readonly IPubSubClient _client = fixture.Client;
    private readonly List<string> _createdTopics = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
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

    [Fact]
    public async Task PublishAsync_SucceedsWithValidTopic()
    {
        var topic = UniqueTopic();
        await _client.CreateTopicAsync(topic);

        var act = () => _client.PublishAsync(topic, new TestMessage("hello"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_ThrowsWhenTopicNotExists()
    {
        var act = () => _client.PublishAsync($"non-existent-{Guid.NewGuid():N}", new TestMessage("hello"));

        await act.Should().ThrowAsync<RpcException>();
    }
}
