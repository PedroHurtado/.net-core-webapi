namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp)
        {
            ConflictGuard.ThrowIf(!externalApp.IsActive, "External app is already inactive");

            externalApp.IsActive = false;

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
