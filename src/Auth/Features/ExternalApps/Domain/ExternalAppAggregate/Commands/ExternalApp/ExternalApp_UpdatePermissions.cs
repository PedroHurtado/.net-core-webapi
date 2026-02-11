namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public record UpdateExternalAppPermissionsCommand(
    List<string> Groups,
    List<string> AdditionalScopes,
    List<string> ExcludedScopes
);

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdatePermissions(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<UpdateExternalAppPermissionsCommand, ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp, UpdateExternalAppPermissionsCommand command)
        {
            externalApp._groups = command.Groups.ToHashSet();
            externalApp._additionalScopes = command.AdditionalScopes.ToHashSet();
            externalApp._excludedScopes = command.ExcludedScopes.ToHashSet();

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
