namespace Fudie.PubSub;

public interface IPublisher
{
    Task PublishAsync<T>(string topicId, T message);
}
