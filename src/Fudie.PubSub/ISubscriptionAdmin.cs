namespace Fudie.PubSub;

public interface ISubscriptionAdmin
{
    Task CreateSubscriptionAsync(string subscriptionId, string topicId);
    Task DeleteSubscriptionAsync(string subscriptionId);
    Task<bool> SubscriptionExistsAsync(string subscriptionId);
}
