namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public record UpdateExternalAppCommand(
    string Name
);

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractModifyCommand<UpdateExternalAppCommand, ExternalApp>
    {
        public override ExternalApp Execute(ExternalApp externalApp, UpdateExternalAppCommand command)
        {
            externalApp.Name = command.Name;

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
