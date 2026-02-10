namespace Auth.UnitTests.Roles.Domain.TenantRoleAggregateTests.Commands.TenantRoleTests;

public class TenantRoleUpdateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly TenantRole.Update _update = fixture.Get<TenantRole.Update>();

    [Fact]
    public void Execute_WithEditableRole_UpdatesNameAndDescription()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Manager")
            .WithDescription("Encargado. Gestiona operativa diaria.")
            .WithIsSystem(true)
            .WithIsEditable(true)
            .WithIsDeletable(false);

        var command = new UpdateTenantRoleCommand(
            Name: "Encargado Senior",
            Description: "Coordina servicio");

        var result = _update.Execute(role, command);

        result.Name.Should().Be("Encargado Senior");
        result.Description.Should().Be("Coordina servicio");
    }

    [Fact]
    public void Execute_WhenNotEditable_ThrowsConflictException()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Owner")
            .WithDescription("Propietario.")
            .WithIsSystem(true)
            .WithIsEditable(false)
            .WithIsDeletable(false);

        var command = new UpdateTenantRoleCommand(
            Name: "Nuevo nombre",
            Description: "Nueva descripción");

        var act = () => _update.Execute(role, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*This role cannot be edited*");
    }
}
