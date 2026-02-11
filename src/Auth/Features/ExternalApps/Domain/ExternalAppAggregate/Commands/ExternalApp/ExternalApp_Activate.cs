namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp)
        {
            ConflictGuard.ThrowIf(externalApp.IsActive, "External app is already active");

            externalApp.IsActive = true;

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
