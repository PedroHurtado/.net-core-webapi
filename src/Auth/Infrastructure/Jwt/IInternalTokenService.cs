namespace Auth.Infrastructure.Jwt;

public record SessionTokenData(
    Guid UserId,
    Guid? TenantId,
    bool IsOwner,
    string[] Groups,
    string[] AdditionalScopes,
    string[] ExcludedScopes);

public interface IInternalTokenService
{
    string GenerateTokenInternal(Guid tenantId);
    string GenerateTokenInternal();
    string GenerateSessionToken(SessionTokenData data);
}
