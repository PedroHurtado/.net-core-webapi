namespace Auth.UnitTests.Roles.Domain.TenantRoleAggregateTests.Commands.TenantRoleTests;

public class TenantRoleCreateOwnerRoleTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly TenantRole.CreateOwnerRole _createOwnerRole = fixture.Get<TenantRole.CreateOwnerRole>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsOwnerRole()
    {
        var tenantId = Guid.NewGuid();
        var command = new CreateOwnerRoleCommand(TenantId: tenantId);

        var result = _createOwnerRole.Execute(command);

        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Owner");
        result.Description.Should().Be("Propietario. Acceso total al tenant.");
        result.IsOwner.Should().BeTrue();
        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
    }
}
