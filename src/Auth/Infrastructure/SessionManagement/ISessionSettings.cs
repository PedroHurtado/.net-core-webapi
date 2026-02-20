namespace Auth.Infrastructure.SessionManagement;

public record SessionSettings(
    string CookieName,
    int ExpirationDays
);

public interface ISessionSettings
{
    SessionSettings Get();
}
