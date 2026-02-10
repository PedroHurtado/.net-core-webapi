namespace Auth.Features.Sessions.Domain.SessionAggregate;

public record SetTenantContextCommand(
    Guid TenantId,
    Guid RoleId,
    string[] Groups,
    string[] AdditionalScopes,
    string[] ExcludedScopes,
    bool IsOwner
);

public partial class Session
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SetTenantContext(
        IValidator<Session> sessionValidator
    ) : AbstractModifyCommand<SetTenantContextCommand, Session>
    {
        public override Session Execute(Session session, SetTenantContextCommand command)
        {
            session.TenantId = command.TenantId;
            session.RoleId = command.RoleId;
            session.Groups = command.Groups;
            session.AdditionalScopes = command.AdditionalScopes;
            session.ExcludedScopes = command.ExcludedScopes;
            session.IsOwner = command.IsOwner;

            return sessionValidator.ValidateOrThrow(session);
        }
    }
}
