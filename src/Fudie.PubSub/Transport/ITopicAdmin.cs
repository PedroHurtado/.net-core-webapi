namespace Fudie.PubSub.Transport;

public interface ITopicAdmin
{
    Task CreateTopicAsync(string topicId);
    Task DeleteTopicAsync(string topicId);
    Task<bool> TopicExistsAsync(string topicId);
}
