namespace Auth.Features.Memberships.Domain.MembershipAggregate;

public record CreateMembershipCommand(
    Guid TenantId,
    string InvitationEmail,
    TenantRole Role
);

public partial class Membership
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Create(
        IValidator<Membership> membershipValidator
    ) : AbstractCreateCommand<CreateMembershipCommand, Membership>
    {
        public override Membership Execute(CreateMembershipCommand command)
        {
            var membership = new Membership(Guid.NewGuid())
            {
                TenantId = command.TenantId,
                User = null,
                Role = command.Role,
                IsActive = true,
                InvitationEmail = command.InvitationEmail,
                InvitationStatus = InvitationStatus.Pending
            };

            return membershipValidator.ValidateOrThrow(membership);
        }
    }
}
