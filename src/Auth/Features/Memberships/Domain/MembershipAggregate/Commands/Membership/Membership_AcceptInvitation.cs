namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public record AcceptInvitationCommand(
    User User
);

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class AcceptInvitation(
        IValidator<Membership> membershipValidator
    ) : AbstractModifyCommand<AcceptInvitationCommand, Membership>
    {
        public override Membership Execute(Membership membership, AcceptInvitationCommand command)
        {
            ConflictGuard.ThrowIf(
                membership.InvitationStatus != InvitationStatus.Pending,
                "Invitation is not pending");

            membership.User = command.User;
            membership.InvitationStatus = InvitationStatus.Accepted;

            return membershipValidator.ValidateOrThrow(membership);
        }
    }
}
