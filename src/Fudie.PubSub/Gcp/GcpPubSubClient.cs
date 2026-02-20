using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;

namespace Fudie.PubSub.Gcp;

internal sealed class GcpPubSubClient(string projectId) : IPubSubClient
{
    private readonly PublisherServiceApiClient _publisherApi = new PublisherServiceApiClientBuilder
    {
        EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction
    }.Build();

    private readonly SubscriberServiceApiClient _subscriberApi = new SubscriberServiceApiClientBuilder
    {
        EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction
    }.Build();

    public async Task CreateTopicAsync(string topicId)
    {
        var topicName = TopicName.FromProjectTopic(projectId, topicId);
        await _publisherApi.CreateTopicAsync(topicName);
    }

    public async Task DeleteTopicAsync(string topicId)
    {
        var topicName = TopicName.FromProjectTopic(projectId, topicId);
        await _publisherApi.DeleteTopicAsync(topicName);
    }

    public async Task<bool> TopicExistsAsync(string topicId)
    {
        try
        {
            var topicName = TopicName.FromProjectTopic(projectId, topicId);
            await _publisherApi.GetTopicAsync(topicName);
            return true;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task CreateSubscriptionAsync(string subscriptionId, string topicId)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
        var topicName = TopicName.FromProjectTopic(projectId, topicId);
        await _subscriberApi.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);
    }

    public async Task DeleteSubscriptionAsync(string subscriptionId)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
        await _subscriberApi.DeleteSubscriptionAsync(subscriptionName);
    }

    public async Task<bool> SubscriptionExistsAsync(string subscriptionId)
    {
        try
        {
            var subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
            await _subscriberApi.GetSubscriptionAsync(subscriptionName);
            return true;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task PublishAsync(string topicId, byte[] data)
    {
        var topicName = TopicName.FromProjectTopic(projectId, topicId);
        var message = new PubsubMessage { Data = ByteString.CopyFrom(data) };
        await _publisherApi.PublishAsync(topicName, [message]);
    }

    public async Task SubscribeAsync(string subscriptionId, Func<byte[], CancellationToken, Task> handler, CancellationToken ct = default)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
        var subscriber = await new SubscriberClientBuilder
        {
            SubscriptionName = subscriptionName,
            EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction
        }.BuildAsync(ct);

        await subscriber.StartAsync(async (message, token) =>
        {
            await handler(message.Data.ToByteArray(), token);
            return SubscriberClient.Reply.Ack;
        });
    }
}
