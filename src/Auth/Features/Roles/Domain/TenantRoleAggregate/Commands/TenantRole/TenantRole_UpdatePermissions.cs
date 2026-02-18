namespace Auth.Features.Roles.Domain.TenantRoleAggregate;

public record UpdatePermissionsCommand(
    List<string> Groups,
    List<string> AdditionalScopes,
    List<string> ExcludedScopes
);

public partial class TenantRole
{
    [Injectable(ServiceLifetime.Singleton)]
    public class UpdatePermissions(
        IValidator<TenantRole> tenantRoleValidator
    ) : AbstractModifyCommand<UpdatePermissionsCommand, TenantRole>
    {
        public override TenantRole Execute(TenantRole role, UpdatePermissionsCommand command)
        {
            ConflictGuard.ThrowIf(role.IsOwner, "This role cannot be edited");

            role._groups = [.. command.Groups];
            role._additionalScopes = [.. command.AdditionalScopes];
            role._excludedScopes = [.. command.ExcludedScopes];

            return tenantRoleValidator.ValidateOrThrow(role);
        }
    }
}
