namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class CancelInvitation(
        IValidator<Membership> membershipValidator
    ) : AbstractModifyCommand<Membership>
    {
        public override Membership Execute(Membership membership)
        {
            ConflictGuard.ThrowIf(
                membership.InvitationStatus != InvitationStatus.Pending,
                "Invitation is not pending");

            membership.InvitationStatus = InvitationStatus.Cancelled;

            return membershipValidator.ValidateOrThrow(membership);
        }
    }
}
