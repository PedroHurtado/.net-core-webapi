namespace Auth.Infrastructure.OAuth;

public record OAuthSettings(
    string StateCookieName,
    int StateExpirationSeconds
);

public interface IOAuthSettings
{
    OAuthSettings Get();
}
