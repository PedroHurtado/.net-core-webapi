namespace Auth.Features.Roles.Api.TenantRoleAggregate;

public record TenantRoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsOwner,
    IReadOnlyCollection<string> Groups,
    IReadOnlyCollection<string> AdditionalScopes,
    IReadOnlyCollection<string> ExcludedScopes)
{
    public static TenantRoleResponse Map(TenantRole entity) => new(
        Id: entity.Id,
        Name: entity.Name,
        Description: entity.Description,
        IsOwner: entity.IsOwner,
        Groups: entity.Groups,
        AdditionalScopes: entity.AdditionalScopes,
        ExcludedScopes: entity.ExcludedScopes);
}
