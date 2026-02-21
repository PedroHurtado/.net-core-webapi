namespace Fudie.PubSub.UnitTests.Messaging;

public class MessagePublisherTests
{
    [Fact]
    public async Task PublishAsync_WrapsMessageInEnvelope()
    {
        var stub = new StubPubSubClient();
        var context = new MessageContext();
        var publisher = new MessagePublisher(stub, context);

        await publisher.PublishAsync("topic", new SampleMessage("Pedro", 30));

        stub.LastPublishedData.Should().NotBeNull();
        var envelope = System.Text.Json.JsonSerializer.Deserialize<Envelope<SampleMessage>>(stub.LastPublishedData!);
        envelope!.Payload.Should().Be(new SampleMessage("Pedro", 30));
        envelope.Type.Should().Be("SampleMessage");
    }

    [Fact]
    public async Task PublishAsync_GeneratesUniqueMessageId()
    {
        var stub = new StubPubSubClient();
        var publisher = new MessagePublisher(stub, new MessageContext());

        await publisher.PublishAsync("topic", new SampleMessage("Pedro", 30));

        var envelope = System.Text.Json.JsonSerializer.Deserialize<Envelope<SampleMessage>>(stub.LastPublishedData!);
        envelope!.MessageId.Should().NotBeNullOrEmpty();
        Guid.TryParse(envelope.MessageId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_IncludesClaimsFromContext()
    {
        var stub = new StubPubSubClient();
        var context = new MessageContext();
        context.Populate(
            new Dictionary<string, string> { ["tenant_id"] = "t-1", ["sub"] = "user-1" },
            "corr-42"
        );
        var publisher = new MessagePublisher(stub, context);

        await publisher.PublishAsync("topic", new SampleMessage("Pedro", 30));

        var envelope = System.Text.Json.JsonSerializer.Deserialize<Envelope<SampleMessage>>(stub.LastPublishedData!);
        envelope!.CorrelationId.Should().Be("corr-42");
        envelope.Claims.Should().ContainKey("tenant_id").WhoseValue.Should().Be("t-1");
        envelope.Claims.Should().ContainKey("sub").WhoseValue.Should().Be("user-1");
    }

    [Fact]
    public async Task PublishAsync_NullClaimsWhenContextEmpty()
    {
        var stub = new StubPubSubClient();
        var publisher = new MessagePublisher(stub, new MessageContext());

        await publisher.PublishAsync("topic", new SampleMessage("Pedro", 30));

        var envelope = System.Text.Json.JsonSerializer.Deserialize<Envelope<SampleMessage>>(stub.LastPublishedData!);
        envelope!.Claims.Should().BeNull();
        envelope.CorrelationId.Should().BeNull();
    }

    [Fact]
    public async Task PublishAsync_SetsOccurredAt()
    {
        var stub = new StubPubSubClient();
        var publisher = new MessagePublisher(stub, new MessageContext());
        var before = DateTime.UtcNow;

        await publisher.PublishAsync("topic", new SampleMessage("Pedro", 30));

        var after = DateTime.UtcNow;
        var envelope = System.Text.Json.JsonSerializer.Deserialize<Envelope<SampleMessage>>(stub.LastPublishedData!);
        envelope!.OccurredAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
