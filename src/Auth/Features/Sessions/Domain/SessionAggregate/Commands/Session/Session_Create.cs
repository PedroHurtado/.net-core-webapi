namespace Auth.Features.Sessions.Domain.SessionAggregate;

public record CreateSessionCommand(
    Guid UserId
);

public partial class Session
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Session> sessionValidator
    ) : AbstractCreateCommand<CreateSessionCommand, Session>
    {
        public override Session Execute(CreateSessionCommand command)
        {
            var now = DateTimeOffset.UtcNow;

            var session = new Session(Guid.NewGuid())
            {
                UserId = command.UserId,
                TenantId = null,
                RoleId = null,
                Groups = [],
                AdditionalScopes = [],
                ExcludedScopes = [],
                IsOwner = false,
                CreatedAt = now,
                LastActivityAt = now,
                ExpiresAt = now.AddDays(30)
            };

            return sessionValidator.ValidateOrThrow(session);
        }
    }
}
