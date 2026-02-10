namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

public record SeedSystemRolesCommand(
    Guid TenantId
);

public partial class TenantRole
{
    [Injectable(ServiceLifetime.Singleton)]
    public class SeedSystemRoles(
        IValidator<TenantRole> tenantRoleValidator
    ) : AbstractCreateCommand<SeedSystemRolesCommand, List<TenantRole>>
    {
        public override List<TenantRole> Execute(SeedSystemRolesCommand command)
        {
            var roles = new List<TenantRole>
            {
                new TenantRole(Guid.NewGuid())
                {
                    TenantId = command.TenantId,
                    Name = "Manager",
                    Description = "Encargado. Gestiona operativa diaria.",
                    IsSystem = true,
                    IsEditable = true,
                    IsDeletable = false
                },
                new TenantRole(Guid.NewGuid())
                {
                    TenantId = command.TenantId,
                    Name = "Waiter",
                    Description = "Camarero. Acceso limitado a operaciones del día a día.",
                    IsSystem = true,
                    IsEditable = true,
                    IsDeletable = false
                }
            };

            foreach (var role in roles)
                tenantRoleValidator.ValidateOrThrow(role);

            return roles;
        }
    }
}
