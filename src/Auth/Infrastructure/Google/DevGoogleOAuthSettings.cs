namespace Auth.Infrastructure.Google;

public class DevGoogleOAuthSettings(IConfiguration configuration) : IGoogleOAuthSettings
{
    public GoogleOAuthSettings Get()
    {
        var json = configuration["Google:Oauth"]
            ?? throw new InvalidOperationException("Google:Oauth secret not found");

        var doc = JsonDocument.Parse(json);
        var web = doc.RootElement.GetProperty("web");

        return new GoogleOAuthSettings(
            ClientId: web.GetProperty("client_id").GetString()!,
            ClientSecret: web.GetProperty("client_secret").GetString()!,
            RedirectUri: web.GetProperty("redirect_uris")[0].GetString()!,
            AuthUri: web.GetProperty("auth_uri").GetString()!,
            TokenUri: web.GetProperty("token_uri").GetString()!
        );
    }
}