namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipResendInvitationTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.ResendInvitation _resendInvitation = fixture.Get<Membership.ResendInvitation>();

    [Fact]
    public void Execute_WithPendingInvitation_ReturnsMembership()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var result = _resendInvitation.Execute(membership);

        result.InvitationStatus.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void Execute_WhenNotPending_ThrowsConflictException()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var act = () => _resendInvitation.Execute(membership);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }
}
