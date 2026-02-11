namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipAcceptInvitationTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.AcceptInvitation _acceptInvitation = fixture.Get<Membership.AcceptInvitation>();

    [Fact]
    public void Execute_WithPendingInvitation_AcceptsAndLinksUser()
    {
        var user = new TestableUser(Guid.NewGuid());
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var command = new AcceptInvitationCommand(User: user);

        var result = _acceptInvitation.Execute(membership, command);

        result.User.Should().Be(user);
        result.InvitationStatus.Should().Be(InvitationStatus.Accepted);
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

        var command = new AcceptInvitationCommand(User: new TestableUser(Guid.NewGuid()));

        var act = () => _acceptInvitation.Execute(membership, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }

    [Fact]
    public void Execute_WhenCancelled_ThrowsConflictException()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Cancelled);

        var command = new AcceptInvitationCommand(User: new TestableUser(Guid.NewGuid()));

        var act = () => _acceptInvitation.Execute(membership, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }
}
