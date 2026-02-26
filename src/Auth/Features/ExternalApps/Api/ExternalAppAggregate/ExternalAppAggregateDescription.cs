namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate;

public class ExternalAppAggregateDescription : IAggregateDescription
{
    public string Id => "external-app";
    public string DisplayName => "External Apps";
    public string? Icon => "link";
    public string ReadDescription => "View external apps";
    public string WriteDescription => "Manage external apps";
}
