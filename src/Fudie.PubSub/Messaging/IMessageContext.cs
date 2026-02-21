namespace Fudie.PubSub.Messaging;

public interface IMessageContext
{
    string? TenantId { get; }
    string? UserId { get; }
    string? CorrelationId { get; }
    IDictionary<string, string> Claims { get; }
}
