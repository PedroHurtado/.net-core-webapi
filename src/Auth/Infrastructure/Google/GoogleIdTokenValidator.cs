namespace Auth.Infrastructure.Google;

[Injectable]
public class GoogleIdTokenValidator(
    IGoogleOAuthSettings googleOAuthSettings,
    IGoogleCertificateProvider certificateProvider
) : IGoogleIdTokenValidator
{
    public async Task<GoogleIdTokenClaims> ValidateAsync(string idToken)
    {
        var settings = googleOAuthSettings.Get();
        var keys = await certificateProvider.GetSigningKeysAsync();

        var handler = new JsonWebTokenHandler();
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKeys = keys,
            ValidAudience = settings.ClientId,
            ValidIssuers = ["https://accounts.google.com", "accounts.google.com"],
            ValidateLifetime = true
        };

        var result = await handler.ValidateTokenAsync(idToken, parameters);

        if (!result.IsValid)
            throw new SecurityTokenValidationException("Invalid Google id_token", result.Exception);

        return new GoogleIdTokenClaims(
            Sub: result.Claims["sub"].ToString()!,
            Email: result.Claims["email"].ToString()!,
            Name: result.Claims["name"].ToString()!,
            Picture: result.Claims.TryGetValue("picture", out var pic) ? pic?.ToString() : null
        );
    }
}
