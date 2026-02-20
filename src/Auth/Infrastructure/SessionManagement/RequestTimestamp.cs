namespace Auth.Infrastructure.SessionManagement;

[Injectable]
public class RequestTimestamp(ISessionSettings sessionSettings) : IRequestTimestamp
{
    private readonly SessionSettings _settings = sessionSettings.Get();

    public DateTime UtcNow { get; } = DateTime.UtcNow;

    public DateTime ExpiresAt => UtcNow.AddDays(_settings.ExpirationDays);
}
