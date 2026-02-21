namespace Fudie.PubSub.Messaging;

public record Envelope<T>(
    string MessageId,
    string? CorrelationId,
    string Type,
    DateTime OccurredAt,
    IDictionary<string, string>? Claims,
    T Payload
);
