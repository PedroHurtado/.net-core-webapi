namespace Auth.Infrastructure.Jwt;

public interface IJwtKeyProvider
{
    ECDsa GetPrivateKey();
    JsonWebKey GetJsonWebKey();
}