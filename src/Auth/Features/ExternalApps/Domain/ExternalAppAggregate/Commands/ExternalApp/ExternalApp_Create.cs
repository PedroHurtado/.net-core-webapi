namespace Auth.Features.ExternalApps.Domain.ExternalAppAggregate;

public record CreateExternalAppCommand(
    Guid TenantId,
    string Name,
    string InvitationEmail
);

public partial class ExternalApp
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<ExternalApp> externalAppValidator
    ) : AbstractCreateCommand<CreateExternalAppCommand, ExternalApp>
    {
        public override ExternalApp Execute(CreateExternalAppCommand command)
        {
            var externalApp = new ExternalApp(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                User = null,
                Name = command.Name,
                IsActive = true,
                InvitationEmail = command.InvitationEmail,
                InvitationStatus = InvitationStatus.Pending,
                ApiKeyHash = null,
                ApiKeyPrefix = null,
                ApiKeyExpiresAt = null
            };

            return externalAppValidator.ValidateOrThrow(externalApp);
        }
    }
}
