namespace Fudie.Gateway.Auth;

public interface IAuthService
{
    [Post("/auth/resolve")]
    Task<HttpResponseMessage> ResolveAuth([Header("Cookie")] string cookie);

    [Post("/auth/resolve-api-key")]
    Task<HttpResponseMessage> ResolveApiKey([HeaderCollection] IDictionary<string, string> headers);
}
