namespace Auth.Features.Roles.Api.TenantRoleAggregate;

public record TenantRoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsEditable,
    bool IsDeletable,
    IReadOnlyCollection<string> Groups,
    IReadOnlyCollection<string> AdditionalScopes,
    IReadOnlyCollection<string> ExcludedScopes)
{
    public static TenantRoleResponse Map(TenantRole entity) => new(
        Id: entity.Id,
        Name: entity.Name,
        Description: entity.Description,
        IsSystem: entity.IsSystem,
        IsEditable: entity.IsEditable,
        IsDeletable: entity.IsDeletable,
        Groups: entity.Groups,
        AdditionalScopes: entity.AdditionalScopes,
        ExcludedScopes: entity.ExcludedScopes);
}
