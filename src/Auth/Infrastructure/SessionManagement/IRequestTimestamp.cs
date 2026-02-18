namespace Auth.Infrastructure.SessionManagement;

public interface IRequestTimestamp
{
    DateTime UtcNow { get; }
    DateTime ExpiresAt { get; }
}
