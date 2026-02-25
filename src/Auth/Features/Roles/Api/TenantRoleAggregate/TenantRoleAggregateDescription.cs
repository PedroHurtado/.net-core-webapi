namespace Auth.Features.Roles.Api.TenantRoleAggregate;

public class TenantRoleAggregateDescription : IAggregateDescription
{
    public string Id => "tenant-role";
    public string DisplayName => "Roles";
    public string? Icon => "shield";
}
