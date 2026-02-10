namespace Auth.Features.Sessions.Domain.SessionAggregate;

public partial class Session
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ClearTenantContext(
        IValidator<Session> sessionValidator
    ) : AbstractModifyCommand<Session>
    {
        public override Session Execute(Session session)
        {
            session.TenantId = null;
            session.RoleId = null;
            session.Groups = [];
            session.AdditionalScopes = [];
            session.ExcludedScopes = [];
            session.IsOwner = false;

            return sessionValidator.ValidateOrThrow(session);
        }
    }
}
