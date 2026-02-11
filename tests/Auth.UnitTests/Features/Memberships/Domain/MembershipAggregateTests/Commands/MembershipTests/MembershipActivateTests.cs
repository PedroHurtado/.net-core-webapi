namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipActivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.Activate _activate = fixture.Get<Membership.Activate>();

    [Fact]
    public void Execute_WithInactiveMembership_ActivatesMembership()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(false)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var result = _activate.Execute(membership);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var act = () => _activate.Execute(membership);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Membership is already active*");
    }
}
