namespace Auth.Features.Memberships.Api.MembershipAggregate;

public class MembershipAggregateDescription : IAggregateDescription
{
    public string Id => "membership";
    public string DisplayName => "Membresías";
    public string? Icon => "user-plus";
}
