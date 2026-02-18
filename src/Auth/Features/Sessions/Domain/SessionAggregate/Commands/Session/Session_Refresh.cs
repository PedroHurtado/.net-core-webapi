namespace Auth.Features.Sessions.Domain.SessionAggregate;

public record RefreshSessionCommand(
    DateTime Now,
    DateTime ExpiresAt
);

public partial class Session
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Refresh(
        IValidator<Session> sessionValidator
    ) : AbstractModifyCommand<RefreshSessionCommand, Session>
    {
        public override Session Execute(Session session, RefreshSessionCommand command)
        {
            UnauthorizedGuard.ThrowIf(session.IsExpired, "Session expired");

            session.LastActivityAt = command.Now;
            session.ExpiresAt = command.ExpiresAt;

            return sessionValidator.ValidateOrThrow(session);
        }
    }
}
