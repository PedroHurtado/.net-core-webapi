namespace Fudie.Security;

public interface IJwksApi
{
    [Get("/auth/jwks")]
    Task<JwksResponse> GetJwksAsync();
}
