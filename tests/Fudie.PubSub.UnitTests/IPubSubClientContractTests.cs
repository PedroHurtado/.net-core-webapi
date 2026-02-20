namespace Fudie.PubSub.UnitTests;

public abstract class IPubSubClientContractTests
{
    protected abstract IPubSubClient CreateClient();

    [Fact]
    public async Task CreateTopicAsync_DoesNotThrow()
    {
        var client = CreateClient();

        var act = () => client.CreateTopicAsync("test-topic");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task TopicExistsAsync_ReturnsFalse_WhenTopicDoesNotExist()
    {
        var client = CreateClient();

        var exists = await client.TopicExistsAsync("non-existent-topic");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TopicExistsAsync_ReturnsTrue_AfterCreating()
    {
        var client = CreateClient();
        await client.CreateTopicAsync("exists-topic");

        var exists = await client.TopicExistsAsync("exists-topic");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTopicAsync_RemovesTopic()
    {
        var client = CreateClient();
        await client.CreateTopicAsync("delete-topic");

        await client.DeleteTopicAsync("delete-topic");

        var exists = await client.TopicExistsAsync("delete-topic");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSubscriptionAsync_DoesNotThrow()
    {
        var client = CreateClient();
        await client.CreateTopicAsync("sub-topic");

        var act = () => client.CreateSubscriptionAsync("test-sub", "sub-topic");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscriptionExistsAsync_ReturnsFalse_WhenNotExists()
    {
        var client = CreateClient();

        var exists = await client.SubscriptionExistsAsync("non-existent-sub");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SubscriptionExistsAsync_ReturnsTrue_AfterCreating()
    {
        var client = CreateClient();
        await client.CreateTopicAsync("sub-exists-topic");
        await client.CreateSubscriptionAsync("exists-sub", "sub-exists-topic");

        var exists = await client.SubscriptionExistsAsync("exists-sub");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_RemovesSubscription()
    {
        var client = CreateClient();
        await client.CreateTopicAsync("del-sub-topic");
        await client.CreateSubscriptionAsync("del-sub", "del-sub-topic");

        await client.DeleteSubscriptionAsync("del-sub");

        var exists = await client.SubscriptionExistsAsync("del-sub");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_DoesNotThrow()
    {
        var client = CreateClient();
        await client.CreateTopicAsync("pub-topic");

        var act = () => client.PublishAsync("pub-topic", new { Text = "test" });

        await act.Should().NotThrowAsync();
    }
}
