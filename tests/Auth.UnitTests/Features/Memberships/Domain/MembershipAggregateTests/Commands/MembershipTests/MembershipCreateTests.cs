namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests.Commands.MembershipTests;

public class MembershipCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Membership.Create _create = fixture.Get<Membership.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsMembership()
    {
        var role = new TestableTenantRole(Guid.NewGuid());
        var tenantId = Guid.NewGuid();
        var command = new CreateMembershipCommand(
            TenantId: tenantId,
            InvitationEmail: "maria@ejemplo.com",
            Role: role);

        var result = _create.Execute(command);

        result.TenantId.Should().Be(tenantId);
        result.InvitationEmail.Should().Be("maria@ejemplo.com");
        result.Role.Should().Be(role);
        result.User.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.InvitationStatus.Should().Be(InvitationStatus.Pending);
    }
}
