namespace Auth.Features.Sessions.Domain.SessionAggregate;

public record CreateSessionCommand(
    Guid UserId,
    DateTime Now,
    DateTime ExpiresAt
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
            var session = new Session(Guid.NewGuid())
            {
                UserId = command.UserId,
                TenantId = null,
                RoleId = null,
                Groups = [],
                AdditionalScopes = [],
                ExcludedScopes = [],
                IsOwner = false,
                CreatedAt = command.Now,
                LastActivityAt = command.Now,
                ExpiresAt = command.ExpiresAt
            };

            return sessionValidator.ValidateOrThrow(session);
        }
    }
}
