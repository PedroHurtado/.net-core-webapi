namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class ResendInvitation : AbstractModifyCommand<Membership>
    {
        public override Membership Execute(Membership membership)
        {
            ConflictGuard.ThrowIf(
                membership.InvitationStatus == InvitationStatus.Accepted,
                "Invitation is already accepted");

            membership.InvitationStatus = InvitationStatus.Pending;

            return membership;
        }
    }
}
