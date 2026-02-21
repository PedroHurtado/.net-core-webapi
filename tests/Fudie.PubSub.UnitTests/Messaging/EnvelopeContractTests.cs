namespace Fudie.PubSub.UnitTests.Messaging;

public class EnvelopeContractTests
{
    [Fact]
    public void Envelope_HasExpectedProperties()
    {
        var claims = new Dictionary<string, string> { ["tenant_id"] = "t-1" };

        var envelope = new Envelope<SampleMessage>(
            MessageId: "msg-1",
            CorrelationId: "corr-1",
            Type: nameof(SampleMessage),
            OccurredAt: new DateTime(2026, 2, 21, 12, 0, 0, DateTimeKind.Utc),
            Claims: claims,
            Payload: new SampleMessage("Pedro", 30)
        );

        envelope.MessageId.Should().Be("msg-1");
        envelope.CorrelationId.Should().Be("corr-1");
        envelope.Type.Should().Be("SampleMessage");
        envelope.OccurredAt.Should().Be(new DateTime(2026, 2, 21, 12, 0, 0, DateTimeKind.Utc));
        envelope.Claims.Should().ContainKey("tenant_id").WhoseValue.Should().Be("t-1");
        envelope.Payload.Should().Be(new SampleMessage("Pedro", 30));
    }

    [Fact]
    public void Envelope_AllowsNullOptionalFields()
    {
        var envelope = new Envelope<SampleMessage>(
            MessageId: "msg-2",
            CorrelationId: null,
            Type: nameof(SampleMessage),
            OccurredAt: DateTime.UtcNow,
            Claims: null,
            Payload: new SampleMessage("Claude", 99)
        );

        envelope.CorrelationId.Should().BeNull();
        envelope.Claims.Should().BeNull();
        envelope.Payload.Should().NotBeNull();
    }

    [Fact]
    public void Envelope_IsSerializableRoundtrip()
    {
        var original = new Envelope<SampleMessage>(
            MessageId: "msg-3",
            CorrelationId: "corr-3",
            Type: nameof(SampleMessage),
            OccurredAt: new DateTime(2026, 2, 21, 12, 0, 0, DateTimeKind.Utc),
            Claims: new Dictionary<string, string> { ["sub"] = "user-1", ["tenant_id"] = "t-1" },
            Payload: new SampleMessage("Pedro", 30)
        );

        var serializer = new JsonPubSubSerializer();
        var bytes = serializer.Serialize(original);
        var deserialized = serializer.Deserialize<Envelope<SampleMessage>>(bytes);

        deserialized.MessageId.Should().Be(original.MessageId);
        deserialized.CorrelationId.Should().Be(original.CorrelationId);
        deserialized.Type.Should().Be(original.Type);
        deserialized.OccurredAt.Should().Be(original.OccurredAt);
        deserialized.Payload.Should().Be(original.Payload);
        deserialized.Claims.Should().BeEquivalentTo(original.Claims);
    }
}
