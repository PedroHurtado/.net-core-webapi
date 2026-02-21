namespace Fudie.PubSub.UnitTests;

public class PubSubClientSerializerTests
{
    [Fact]
    public async Task PublishAsync_UsesDefaultJsonSerializer_WhenNoSerializerProvided()
    {
        var client = new StubPubSubClient();
        IPubSubClient pubsub = client;
        var original = new SampleMessage("Pedro", 30);

        await pubsub.PublishAsync("topic", original);

        var deserialized = System.Text.Json.JsonSerializer.Deserialize<SampleMessage>(client.LastPublishedData!);
        deserialized.Should().Be(original);
    }

    [Fact]
    public async Task PublishAsync_UsesCustomSerializer_WhenProvided()
    {
        var spy = new SpySerializer();
        var client = new StubPubSubClient(spy);
        IPubSubClient pubsub = client;

        await pubsub.PublishAsync("topic", new SampleMessage("Pedro", 30));

        spy.SerializeCalled.Should().BeTrue();
    }

    [Fact]
    public async Task SubscribeAsync_UsesCustomSerializer_ForDeserialization()
    {
        var spy = new SpySerializer();
        var original = new SampleMessage("Pedro", 30);
        var data = new JsonPubSubSerializer().Serialize(original);
        var client = new StubPubSubClient(spy) { DataToDeliver = data };
        IPubSubClient pubsub = client;

        SampleMessage? received = null;
        await pubsub.SubscribeAsync<SampleMessage>("sub", (msg, _) =>
        {
            received = msg;
            return Task.CompletedTask;
        });

        spy.DeserializeCalled.Should().BeTrue();
        received.Should().Be(original);
    }
}

internal sealed class SpySerializer : ISerializer
{
    public bool SerializeCalled { get; private set; }
    public bool DeserializeCalled { get; private set; }

    public byte[] Serialize<T>(T value)
    {
        SerializeCalled = true;
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
    }

    public T Deserialize<T>(byte[] data)
    {
        DeserializeCalled = true;
        return System.Text.Json.JsonSerializer.Deserialize<T>(data)!;
    }
}
