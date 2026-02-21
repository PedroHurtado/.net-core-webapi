namespace Fudie.PubSub.Gcp.IntegrationTests;

public class TopicTests(GcpFixture fixture) : IClassFixture<GcpFixture>, IAsyncLifetime
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
    public async Task CreateTopicAsync_CreatesSuccessfully()
    {
        var topic = UniqueTopic();

        await _client.CreateTopicAsync(topic);

        var exists = await _client.TopicExistsAsync(topic);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTopicAsync_ThrowsWhenDuplicate()
    {
        var topic = UniqueTopic();
        await _client.CreateTopicAsync(topic);

        var act = () => _client.CreateTopicAsync(topic);

        await act.Should().ThrowAsync<RpcException>();
    }

    [Fact]
    public async Task TopicExistsAsync_ReturnsFalse_WhenNotExists()
    {
        var exists = await _client.TopicExistsAsync($"non-existent-{Guid.NewGuid():N}");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TopicExistsAsync_ReturnsTrue_AfterCreating()
    {
        var topic = UniqueTopic();
        await _client.CreateTopicAsync(topic);

        var exists = await _client.TopicExistsAsync(topic);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTopicAsync_RemovesTopic()
    {
        var topic = UniqueTopic();
        await _client.CreateTopicAsync(topic);

        await _client.DeleteTopicAsync(topic);

        var exists = await _client.TopicExistsAsync(topic);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTopicAsync_ThrowsWhenNotExists()
    {
        var act = () => _client.DeleteTopicAsync($"non-existent-{Guid.NewGuid():N}");

        await act.Should().ThrowAsync<RpcException>();
    }
}
