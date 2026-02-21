namespace Fudie.PubSub.Messaging;

public class MessageContext : IMessageContext
{
    public string? TenantId { get; private set; }
    public string? UserId { get; private set; }
    public string? CorrelationId { get; private set; }
    public IDictionary<string, string> Claims { get; private set; } = new Dictionary<string, string>();

    public void Populate(IDictionary<string, string>? claims, string? correlationId)
    {
        CorrelationId = correlationId;

        if (claims is null) return;

        Claims = new Dictionary<string, string>(claims);
        claims.TryGetValue("tenant_id", out var tenantId);
        TenantId = tenantId;
        claims.TryGetValue("sub", out var userId);
        UserId = userId;
    }
}
