namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public record RotateExternalAppApiKeyCommand(
    string ApiKeyHash,
    string ApiKeySalt,
    string ApiKeyPrefix
);

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class RotateApiKey(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<RotateExternalAppApiKeyCommand, ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp, RotateExternalAppApiKeyCommand command)
        {
            ConflictGuard.ThrowIf(
                externalApp.InvitationStatus != InvitationStatus.Accepted,
                "External app invitation has not been accepted");

            ConflictGuard.ThrowIf(
                !externalApp.IsActive,
                "Cannot rotate key for inactive external app");

            externalApp.ApiKeyHash = command.ApiKeyHash;
            externalApp.ApiKeySalt = command.ApiKeySalt;
            externalApp.ApiKeyPrefix = command.ApiKeyPrefix;

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
