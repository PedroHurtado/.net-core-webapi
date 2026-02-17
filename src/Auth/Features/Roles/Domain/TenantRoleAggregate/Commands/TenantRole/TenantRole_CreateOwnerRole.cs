namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

public record CreateOwnerRoleCommand(
    Guid TenantId
);

public partial class TenantRole
{
    [Injectable(ServiceLifetime.Singleton)]
    public class CreateOwnerRole(
        IValidator<TenantRole> tenantRoleValidator
    ) : AbstractCreateCommand<CreateOwnerRoleCommand, TenantRole>
    {
        public override TenantRole Execute(CreateOwnerRoleCommand command)
        {
            var role = new TenantRole(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                Name = "Owner",
                Description = "Propietario. Acceso total al tenant.",
                IsSystem = true,
                IsEditable = false,
                IsDeletable = false
            };

            return tenantRoleValidator.ValidateOrThrow(role);
        }
    }
}
