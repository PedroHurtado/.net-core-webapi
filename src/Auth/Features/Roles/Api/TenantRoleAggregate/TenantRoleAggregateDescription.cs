namespace Auth.Features.Roles.Api.TenantRoleAggregate;

public class TenantRoleAggregateDescription : IAggregateDescription
{
    public string Id => "tenant-role";
    public string DisplayName => "Roles";
    public string? Icon => "shield";
    public string ReadDescription => "View roles";
    public string WriteDescription => "Manage roles";
}
