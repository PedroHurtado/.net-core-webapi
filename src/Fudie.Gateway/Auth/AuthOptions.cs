namespace Fudie.Gateway.Auth;

public sealed class AuthOptions
{
    public string CookieName { get; init; } = "fudie_session";
    public string Idp { get; init; } = "auth-cluster";
    public string AuthTokenHeader { get; init; } = "X-Auth-Token";
    public string ApiKeyHeader { get; init; } = "X-Api-Key";
}
