namespace Fudie.PubSub.UnitTests;

public record SampleMessage(string Name, int Age);

public class JsonPubSubSerializerTests
{
    private readonly JsonPubSubSerializer _serializer = new();

    [Fact]
    public void Serialize_ReturnsNonEmptyBytes()
    {
        var data = _serializer.Serialize(new SampleMessage("Pedro", 30));

        data.Should().NotBeEmpty();
    }

    [Fact]
    public void Deserialize_ReturnsOriginalRecord()
    {
        var original = new SampleMessage("Pedro", 30);
        var data = _serializer.Serialize(original);

        var result = _serializer.Deserialize<SampleMessage>(data);

        result.Should().Be(original);
    }

    [Fact]
    public void Roundtrip_PreservesAllProperties()
    {
        var original = new SampleMessage("Claude", 99);
        var bytes = _serializer.Serialize(original);
        var deserialized = _serializer.Deserialize<SampleMessage>(bytes);

        deserialized.Name.Should().Be("Claude");
        deserialized.Age.Should().Be(99);
    }

    [Fact]
    public void Deserialize_ThrowsOnInvalidJson()
    {
        var invalidData = new byte[] { 0xFF, 0xFE };

        var act = () => _serializer.Deserialize<SampleMessage>(invalidData);

        act.Should().Throw<Exception>();
    }
}
