namespace Fudie.Security;

public record FudieTokenContext(
    Guid UserId,
    Guid? TenantId,
    bool IsOwner,
    string[] Groups,
    string[] AdditionalScopes,
    string[] ExcludedScopes);
