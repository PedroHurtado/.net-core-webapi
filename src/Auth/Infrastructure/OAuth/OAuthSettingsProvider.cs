namespace Auth.Infrastructure.OAuth;

[Injectable]
public class OAuthSettingsProvider(IConfiguration configuration) : IOAuthSettings
{
    public OAuthSettings Get()
    {
        var stateCookieName = configuration["OAuth:StateCookieName"]
            ?? throw new InvalidOperationException("OAuth:StateCookieName not found");

        var stateExpirationSeconds = int.Parse(
            configuration["OAuth:StateExpirationSeconds"]
                ?? throw new InvalidOperationException("OAuth:StateExpirationSeconds not found"),
            CultureInfo.InvariantCulture);

        return new OAuthSettings(stateCookieName, stateExpirationSeconds);
    }
}
