namespace Auth.Infrastructure.Google;

public record GoogleOAuthSettings(
    string ClientId,
    string ClientSecret,
    string RedirectUri,
    string AuthUri,
    string TokenUri,
    string CertsUri
);

public interface IGoogleOAuthSettings
{
    GoogleOAuthSettings Get();
}