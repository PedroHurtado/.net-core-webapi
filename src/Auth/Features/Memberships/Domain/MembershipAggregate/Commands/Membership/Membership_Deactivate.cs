namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Deactivate(
        IValidator<Membership> membershipValidator
    ) : AbstractModifyCommand<Membership>
    {
        public override Membership Execute(Membership membership)
        {
            ConflictGuard.ThrowIf(!membership.IsActive, "Membership is already inactive");

            membership.IsActive = false;

            return membershipValidator.ValidateOrThrow(membership);
        }
    }
}
