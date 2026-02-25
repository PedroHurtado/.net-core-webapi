namespace Auth.Features.Sessions.Api.SessionAggregate;

public class SessionAggregateDescription : IAggregateDescription
{
    public string Id => "session";
    public string DisplayName => "Sesiones";
    public string? Icon => "key";
}
