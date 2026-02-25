namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate;

public class ExternalAppAggregateDescription : IAggregateDescription
{
    public string Id => "external-app";
    public string DisplayName => "Apps Externas";
    public string? Icon => "link";
}
