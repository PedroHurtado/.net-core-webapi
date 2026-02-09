namespace Auth.Infrastructure.Google;

public interface IGoogleOAuthApi
{
    [Post("/token")]
    Task<GoogleTokenResponse> ExchangeCodeAsync(
        [Body(BodySerializationMethod.UrlEncoded)] GoogleTokenRequest request);
}

public record GoogleTokenRequest(
    [property: AliasAs("code")] string Code,
    [property: AliasAs("client_id")] string ClientId,
    [property: AliasAs("client_secret")] string ClientSecret,
    [property: AliasAs("redirect_uri")] string RedirectUri,
    [property: AliasAs("grant_type")] string GrantType = "authorization_code"
);

public record GoogleTokenResponse(
    string IdToken,
    string AccessToken,
    string TokenType,
    int ExpiresIn
);
