namespace Fudie.PubSub.Hosting;

public interface IMessageHandler<in T>
{
    Task Handle(T message, CancellationToken ct);
}
