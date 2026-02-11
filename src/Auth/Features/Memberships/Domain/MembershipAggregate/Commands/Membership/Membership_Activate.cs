namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Activate(
        IValidator<Membership> membershipValidator
    ) : AbstractModifyCommand<Membership>
    {
        public override Membership Execute(Membership membership)
        {
            ConflictGuard.ThrowIf(membership.IsActive, "Membership is already active");

            membership.IsActive = true;

            return membershipValidator.ValidateOrThrow(membership);
        }
    }
}
