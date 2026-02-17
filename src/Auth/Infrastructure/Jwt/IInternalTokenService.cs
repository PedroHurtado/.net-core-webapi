namespace Auth.Infrastructure.Jwt;

public interface IInternalTokenService
{
    string GenerateTokenInternal(Guid tenantId);
}
