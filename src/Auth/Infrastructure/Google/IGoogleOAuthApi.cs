namespace Auth.Infrastructure.Google;

public interface IGoogleOAuthApi
{
    [Post("/token")]
    Task<GoogleTokenResponse> ExchangeCodeAsync(
        [Body(BodySerializationMethod.UrlEncoded)] GoogleTokenRequest request);
}

public record GoogleTokenRequest(
    [AliasAs("code")] string Code,
    [AliasAs("client_id")] string ClientId,
    [AliasAs("client_secret")] string ClientSecret,
    [AliasAs("redirect_uri")] string RedirectUri,
    [AliasAs("grant_type")] string GrantType = "authorization_code"
);

public record GoogleTokenResponse(
    [property: JsonPropertyName("id_token")] string IdToken,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
