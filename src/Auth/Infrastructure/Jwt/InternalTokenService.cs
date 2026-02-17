namespace Auth.Infrastructure.Jwt;

[Injectable(ServiceLifetime.Singleton)]
public class InternalTokenService(IJwtKeyProvider jwtKeyProvider) : IInternalTokenService
{
    public string GenerateTokenInternal(Guid tenantId)
    {
        var key = new ECDsaSecurityKey(jwtKeyProvider.GetPrivateKey())
        {
            KeyId = jwtKeyProvider.GetJsonWebKey().Kid
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>
            {
                ["tid"] = tenantId.ToString()
            },
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256)
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }
}
