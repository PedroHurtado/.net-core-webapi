namespace Auth.Infrastructure.Google;

public class GoogleOAuthUrlBuilder(IGoogleOAuthSettings googleOAuthSettings) : IGoogleOAuthUrlBuilder
{
    public GoogleOAuthUrl Build()
    {
        var settings = googleOAuthSettings.Get();
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var queryParams = new Dictionary<string, string?>
        {
            ["client_id"] = settings.ClientId,
            ["redirect_uri"] = settings.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "online",
            ["state"] = state
        };

        var url = QueryHelpers.AddQueryString(settings.AuthUri, queryParams);

        return new GoogleOAuthUrl(url, state);
    }
}
