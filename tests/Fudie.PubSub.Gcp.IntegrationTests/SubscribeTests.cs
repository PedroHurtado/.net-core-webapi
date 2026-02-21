namespace Fudie.PubSub.Gcp.IntegrationTests;

public record ChatMessage(string User, string Content);
public record CatalogUpdate(string Service, string[] Routes);

public class SubscribeTests(GcpFixture fixture) : IClassFixture<GcpFixture>, IAsyncLifetime
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
    public async Task SubscribeAsync_ReceivesPublishedRecord()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);
        await _client.CreateSubscriptionAsync(sub, topic);

        var received = new TaskCompletionSource<ChatMessage>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        _ = _client.SubscribeAsync<ChatMessage>(sub, (msg, ct) =>
        {
            received.TrySetResult(msg);
            return Task.CompletedTask;
        }, cts.Token);

        await Task.Delay(500);
        await _client.PublishAsync(topic, new ChatMessage("Pedro", "hola mundo"));

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        result.User.Should().Be("Pedro");
        result.Content.Should().Be("hola mundo");
        cts.Cancel();
    }

    [Fact]
    public async Task SubscribeAsync_ReceivesCatalogUpdate()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);
        await _client.CreateSubscriptionAsync(sub, topic);

        var received = new TaskCompletionSource<CatalogUpdate>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        _ = _client.SubscribeAsync<CatalogUpdate>(sub, (msg, ct) =>
        {
            received.TrySetResult(msg);
            return Task.CompletedTask;
        }, cts.Token);

        await Task.Delay(500);
        await _client.PublishAsync(topic, new CatalogUpdate("auth", ["/login", "/logout"]));

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        result.Service.Should().Be("auth");
        result.Routes.Should().BeEquivalentTo(["/login", "/logout"]);
        cts.Cancel();
    }

    [Fact]
    public async Task SubscribeAsync_ReceivesMultipleMessages()
    {
        var topic = UniqueTopic();
        var sub = UniqueSub();
        await _client.CreateTopicAsync(topic);
        await _client.CreateSubscriptionAsync(sub, topic);

        var messages = new List<ChatMessage>();
        var allReceived = new TaskCompletionSource();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        _ = _client.SubscribeAsync<ChatMessage>(sub, (msg, ct) =>
        {
            messages.Add(msg);
            if (messages.Count >= 3)
                allReceived.TrySetResult();
            return Task.CompletedTask;
        }, cts.Token);

        await Task.Delay(1000);
        await _client.PublishAsync(topic, new ChatMessage("Pedro", "msg-1"));
        await Task.Delay(200);
        await _client.PublishAsync(topic, new ChatMessage("Claude", "msg-2"));
        await Task.Delay(200);
        await _client.PublishAsync(topic, new ChatMessage("Pedro", "msg-3"));

        await allReceived.Task.WaitAsync(TimeSpan.FromSeconds(30));

        messages.Should().HaveCount(3);
        messages.Select(m => m.Content).Should().BeEquivalentTo(["msg-1", "msg-2", "msg-3"]);
        cts.Cancel();
    }
}
