namespace Auth.Features.Users.Api.UserAggregate;

public class UserAggregateDescription : IAggregateDescription
{
    public string Id => "user";
    public string DisplayName => "Usuarios";
    public string? Icon => "user";
    public string ReadDescription => "View users";
    public string WriteDescription => "Manage users";
}
