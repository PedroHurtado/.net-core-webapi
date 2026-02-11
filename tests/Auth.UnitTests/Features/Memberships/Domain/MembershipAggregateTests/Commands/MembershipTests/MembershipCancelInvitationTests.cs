namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipCancelInvitationTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.CancelInvitation _cancelInvitation = fixture.Get<Membership.CancelInvitation>();

    [Fact]
    public void Execute_WithPendingInvitation_CancelsInvitation()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var result = _cancelInvitation.Execute(membership);

        result.InvitationStatus.Should().Be(InvitationStatus.Cancelled);
    }

    [Fact]
    public void Execute_WhenAlreadyAccepted_ThrowsConflictException()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var act = () => _cancelInvitation.Execute(membership);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }

    [Fact]
    public void Execute_WhenAlreadyCancelled_ThrowsConflictException()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Cancelled);

        var act = () => _cancelInvitation.Execute(membership);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }
}
