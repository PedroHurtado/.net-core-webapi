namespace Auth.Features.Sessions.Domain.SessionAggregate;

public partial class Session
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Refresh(
        IValidator<Session> sessionValidator
    ) : AbstractModifyCommand<Session>
    {
        public override Session Execute(Session session)
        {
            UnauthorizedGuard.ThrowIf(session.IsExpired, "Session expired");

            var now = DateTime.UtcNow;
            session.LastActivityAt = now;
            session.ExpiresAt = now.AddDays(30);

            return sessionValidator.ValidateOrThrow(session);
        }
    }
}
