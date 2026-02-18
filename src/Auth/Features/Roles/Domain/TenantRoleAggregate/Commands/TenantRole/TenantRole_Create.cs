namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

public record CreateTenantRoleCommand(
    Guid TenantId,
    string Name,
    string Description
);

public partial class TenantRole
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<TenantRole> tenantRoleValidator
    ) : AbstractCreateCommand<CreateTenantRoleCommand, TenantRole>
    {
        public override TenantRole Execute(CreateTenantRoleCommand command)
        {
            var role = new TenantRole(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                Name = command.Name,
                Description = command.Description,
                IsOwner = false
            };

            return tenantRoleValidator.ValidateOrThrow(role);
        }
    }
}
