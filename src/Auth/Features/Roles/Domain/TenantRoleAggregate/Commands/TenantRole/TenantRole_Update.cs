namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

public record UpdateTenantRoleCommand(
    string Name,
    string Description
);

public partial class TenantRole
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<TenantRole> tenantRoleValidator
    ) : AbstractModifyCommand<UpdateTenantRoleCommand, TenantRole>
    {
        public override TenantRole Execute(TenantRole role, UpdateTenantRoleCommand command)
        {
            ConflictGuard.ThrowIf(!role.IsEditable, "This role cannot be edited");

            role.Name = command.Name;
            role.Description = command.Description;

            return tenantRoleValidator.ValidateOrThrow(role);
        }
    }
}
