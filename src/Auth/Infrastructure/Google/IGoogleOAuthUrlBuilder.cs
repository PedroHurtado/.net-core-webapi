namespace Auth.Infrastructure.Google;

public record GoogleOAuthUrl(string Url, string State);

public interface IGoogleOAuthUrlBuilder
{
    GoogleOAuthUrl Build();
}
