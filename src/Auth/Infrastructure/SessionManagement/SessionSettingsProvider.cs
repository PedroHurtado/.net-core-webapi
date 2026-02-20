namespace Auth.Infrastructure.SessionManagement;

[Injectable]
public class SessionSettingsProvider(IConfiguration configuration) : ISessionSettings
{
    public SessionSettings Get()
    {
        var cookieName = configuration["Session:CookieName"]
            ?? throw new InvalidOperationException("Session:CookieName not found");

        var expirationDays = int.Parse(
            configuration["Session:ExpirationDays"]
                ?? throw new InvalidOperationException("Session:ExpirationDays not found"),
            CultureInfo.InvariantCulture);

        return new SessionSettings(cookieName, expirationDays);
    }
}
