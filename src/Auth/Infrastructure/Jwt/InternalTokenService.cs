namespace Auth.Infrastructure.Jwt;

[Injectable(ServiceLifetime.Singleton)]
public class InternalTokenService(
    IJwtKeyProvider jwtKeyProvider,
    IConfiguration configuration) : IInternalTokenService
{
    private readonly string _internalSecret = configuration["Fudie:InternalSecret"]
        ?? throw new InvalidOperationException("Fudie:InternalSecret secret not found");

    public string GenerateTokenInternal(Guid tenantId)
    {
        var claims = new Dictionary<string, object>
        {
            ["ik"] = _internalSecret,
            ["tid"] = tenantId.ToString()
        };

        return CreateToken(claims);
    }

    public string GenerateTokenInternal()
    {
        var claims = new Dictionary<string, object>
        {
            ["ik"] = _internalSecret
        };

        return CreateToken(claims);
    }

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

        return CreateToken(claims, TimeSpan.FromSeconds(45));
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
