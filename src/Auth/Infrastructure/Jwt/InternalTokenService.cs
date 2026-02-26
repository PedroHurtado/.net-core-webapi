namespace Auth.Infrastructure.Jwt;

[Injectable(ServiceLifetime.Singleton)]
public class InternalTokenService(
    IJwtKeyProvider jwtKeyProvider,
    IConfiguration configuration) : IInternalTokenService
{
    private readonly TimeSpan _sessionTokenLifetime = TimeSpan.FromSeconds(
        int.TryParse(configuration["Fudie:SessionTokenLifetimeSeconds"], out var seconds)
            ? seconds
            : 180);

    public string GenerateSessionToken(SessionTokenData data)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = data.UserId.ToString()
        };

        if (data.TenantId is not null)
        {
            claims["tid"] = data.TenantId.Value.ToString();

            if (data.IsOwner)
            {
                claims["owner"] = true;
            }
            else
            {
                claims["groups"] = data.Groups;
                claims["add"] = data.AdditionalScopes;
                claims["exc"] = data.ExcludedScopes;
            }
        }

        return CreateToken(claims, _sessionTokenLifetime);
    }

    private string CreateToken(Dictionary<string, object> claims, TimeSpan? lifetime = null)
    {
        var key = new ECDsaSecurityKey(jwtKeyProvider.GetPrivateKey())
        {
            KeyId = jwtKeyProvider.GetJsonWebKey().Kid
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(5)),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256)
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }
}
