namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public record ChangeRoleCommand(
    TenantRole Role
);

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ChangeRole(
        IValidator<Membership> membershipValidator
    ) : AbstractModifyCommand<ChangeRoleCommand, Membership>
    {
        public override Membership Execute(Membership membership, ChangeRoleCommand command)
        {
            membership.Role = command.Role;

            return membershipValidator.ValidateOrThrow(membership);
        }
    }
}
