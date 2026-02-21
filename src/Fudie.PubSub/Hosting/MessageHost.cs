namespace Fudie.PubSub.Hosting;

public class MessageHost(IPubSubClient client, IServiceProvider serviceProvider)
{
    public Task SubscribeAsync<T>(string subscriptionId, CancellationToken ct = default)
    {
        ISubscriber subscriber = client;

        return subscriber.SubscribeAsync<Envelope<T>>(subscriptionId, async (envelope, token) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
            context.Populate(envelope.Claims, envelope.CorrelationId);

            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
            await handler.Handle(envelope.Payload, token);
        }, ct);
    }
}
