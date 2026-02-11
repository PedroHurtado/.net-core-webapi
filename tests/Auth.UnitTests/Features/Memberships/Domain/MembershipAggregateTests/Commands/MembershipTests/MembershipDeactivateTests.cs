namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipDeactivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.Deactivate _deactivate = fixture.Get<Membership.Deactivate>();

    [Fact]
    public void Execute_WithActiveMembership_DeactivatesMembership()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var result = _deactivate.Execute(membership);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ThrowsConflictException()
    {
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(new TestableTenantRole(Guid.NewGuid()))
            .WithIsActive(false)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var act = () => _deactivate.Execute(membership);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Membership is already inactive*");
    }
}
