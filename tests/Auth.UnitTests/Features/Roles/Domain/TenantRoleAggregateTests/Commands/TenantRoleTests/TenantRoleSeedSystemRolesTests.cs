namespace Auth.UnitTests.Roles.Domain.TenantRoleAggregateTests.Commands.TenantRoleTests;

public class TenantRoleSeedSystemRolesTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly TenantRole.SeedSystemRoles _seedSystemRoles = fixture.Get<TenantRole.SeedSystemRoles>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsTwoSystemRoles()
    {
        var tenantId = Guid.NewGuid();
        var command = new SeedSystemRolesCommand(TenantId: tenantId);

        var result = _seedSystemRoles.Execute(command);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r =>
        {
            r.TenantId.Should().Be(tenantId);
            r.IsSystem.Should().BeTrue();
            r.IsEditable.Should().BeTrue();
            r.IsDeletable.Should().BeFalse();
            r.Groups.Should().BeEmpty();
            r.AdditionalScopes.Should().BeEmpty();
            r.ExcludedScopes.Should().BeEmpty();
        });
    }

    [Fact]
    public void Execute_WithValidCommand_CreatesManagerRole()
    {
        var command = new SeedSystemRolesCommand(TenantId: Guid.NewGuid());

        var result = _seedSystemRoles.Execute(command);

        var manager = result.Should().ContainSingle(r => r.Name == "Manager").Subject;
        manager.Description.Should().Be("Encargado. Gestiona operativa diaria.");
    }

    [Fact]
    public void Execute_WithValidCommand_CreatesWaiterRole()
    {
        var command = new SeedSystemRolesCommand(TenantId: Guid.NewGuid());

        var result = _seedSystemRoles.Execute(command);

        var waiter = result.Should().ContainSingle(r => r.Name == "Waiter").Subject;
        waiter.Description.Should().Be("Camarero. Acceso limitado a operaciones del día a día.");
    }
}
