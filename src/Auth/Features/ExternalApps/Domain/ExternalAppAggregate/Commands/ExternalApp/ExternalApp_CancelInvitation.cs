namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class CancelInvitation(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp)
        {
            ConflictGuard.ThrowIf(
                externalApp.InvitationStatus != InvitationStatus.Pending,
                "Invitation is not pending");

            externalApp.InvitationStatus = InvitationStatus.Cancelled;

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
