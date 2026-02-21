namespace Fudie.PubSub.Transport;

public interface IPublisher
{
    Task PublishAsync<T>(string topicId, T message);
}
