namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipChangeRoleTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.ChangeRole _changeRole = fixture.Get<Membership.ChangeRole>();

    [Fact]
    public void Execute_WithValidRole_ChangesRole()
    {
        var originalRole = new TestableTenantRole(Guid.NewGuid());
        var newRole = new TestableTenantRole(Guid.NewGuid());
        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(originalRole)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var command = new ChangeRoleCommand(Role: newRole);

        var result = _changeRole.Execute(membership, command);

        result.Role.Should().Be(newRole);
    }
}
