namespace Fudie.PubSub.Messaging;

public class MessagePublisher(IPubSubClient client, IMessageContext context) : IMessagePublisher
{
    public Task PublishAsync<T>(string topicId, T message)
    {
        var envelope = new Envelope<T>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: context.CorrelationId,
            Type: typeof(T).Name,
            OccurredAt: DateTime.UtcNow,
            Claims: context.Claims.Count > 0 ? new Dictionary<string, string>(context.Claims) : null,
            Payload: message
        );

        IPublisher publisher = client;
        return publisher.PublishAsync(topicId, envelope);
    }
}
