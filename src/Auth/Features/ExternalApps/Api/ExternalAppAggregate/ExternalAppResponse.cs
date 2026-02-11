namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate;

public record ExternalAppResponse(
    Guid Id,
    Guid TenantId,
    Guid? UserId,
    string Name,
    bool IsActive,
    string InvitationEmail,
    string InvitationStatus,
    string? ApiKeyPrefix,
    DateTime? ApiKeyExpiresAt,
    IReadOnlyCollection<string> Groups,
    IReadOnlyCollection<string> AdditionalScopes,
    IReadOnlyCollection<string> ExcludedScopes)
{
    public static ExternalAppResponse Map(ExternalApp entity) => new(
        Id: entity.Id,
        TenantId: entity.TenantId,
        UserId: entity.User?.Id,
        Name: entity.Name,
        IsActive: entity.IsActive,
        InvitationEmail: entity.InvitationEmail,
        InvitationStatus: entity.InvitationStatus.ToString(),
        ApiKeyPrefix: entity.ApiKeyPrefix,
        ApiKeyExpiresAt: entity.ApiKeyExpiresAt,
        Groups: entity.Groups,
        AdditionalScopes: entity.AdditionalScopes,
        ExcludedScopes: entity.ExcludedScopes);
}

public record ApiKeyResponse(string ApiKey, string Prefix, string Message);