namespace Fudie.PubSub.Serialization;

public sealed class JsonPubSubSerializer : ISerializer
{
    public byte[] Serialize<T>(T value) =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);

    public T Deserialize<T>(byte[] data) =>
        System.Text.Json.JsonSerializer.Deserialize<T>(data)
            ?? throw new InvalidOperationException($"Failed to deserialize message to {typeof(T).Name}");
}