namespace Auth.Features.Memberships.Api.MembershipAggregate;

public class MembershipAggregateDescription : IAggregateDescription
{
    public string Id => "membership";
    public string DisplayName => "Memberships";
    public string? Icon => "user-plus";
    public string ReadDescription => "View memberships";
    public string WriteDescription => "Manage memberships";
}
