namespace Fudie.PubSub.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string topicId, T message);
}
