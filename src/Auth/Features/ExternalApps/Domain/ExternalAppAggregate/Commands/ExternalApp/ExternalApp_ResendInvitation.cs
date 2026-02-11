namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ResendInvitation : AbstractModifyCommand<ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp)
        {
            ConflictGuard.ThrowIf(
                externalApp.InvitationStatus == InvitationStatus.Accepted,
                "Invitation is already accepted");

            externalApp.InvitationStatus = InvitationStatus.Pending;

            return externalApp;
        }
    }
}
