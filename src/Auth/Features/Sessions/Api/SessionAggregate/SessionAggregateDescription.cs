namespace Auth.Features.Sessions.Api.SessionAggregate;

public class SessionAggregateDescription : IAggregateDescription
{
    public string Id => "session";
    public string DisplayName => "Sessions";
    public string? Icon => "key";
    public string ReadDescription => "View sessions";
    public string WriteDescription => "Manage sessions";
}
