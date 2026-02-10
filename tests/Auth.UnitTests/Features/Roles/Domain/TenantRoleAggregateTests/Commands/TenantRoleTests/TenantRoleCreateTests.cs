namespace Auth.UnitTests.Roles.Domain.TenantRoleAggregateTests.Commands.TenantRoleTests;

public class TenantRoleCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly TenantRole.Create _create = fixture.Get<TenantRole.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsCustomRole()
    {
        var tenantId = Guid.NewGuid();
        var command = new CreateTenantRoleCommand(
            TenantId: tenantId,
            Name: "Sommelier",
            Description: "Carta de vinos");

        var result = _create.Execute(command);

        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Sommelier");
        result.Description.Should().Be("Carta de vinos");
        result.IsSystem.Should().BeFalse();
        result.IsEditable.Should().BeTrue();
        result.IsDeletable.Should().BeTrue();
        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
    }
}
