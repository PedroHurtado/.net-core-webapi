namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public record AcceptExternalAppInvitationCommand(
    User User,
    string ApiKeyHash,
    string ApiKeySalt,
    string ApiKeyPrefix
);

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AcceptInvitation(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<AcceptExternalAppInvitationCommand, ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp, AcceptExternalAppInvitationCommand command)
        {
            ConflictGuard.ThrowIf(
                externalApp.InvitationStatus != InvitationStatus.Pending,
                "Invitation is not pending");

            externalApp.User = command.User;
            externalApp.InvitationStatus = InvitationStatus.Accepted;
            externalApp.ApiKeyHash = command.ApiKeyHash;
            externalApp.ApiKeySalt = command.ApiKeySalt;
            externalApp.ApiKeyPrefix = command.ApiKeyPrefix;

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
