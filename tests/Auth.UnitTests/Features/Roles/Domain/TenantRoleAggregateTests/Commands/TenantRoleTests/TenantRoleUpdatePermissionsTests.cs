namespace Auth.UnitTests.Roles.Domain.TenantRoleAggregateTests.Commands.TenantRoleTests;

public class TenantRoleUpdatePermissionsTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly TenantRole.UpdatePermissions _updatePermissions = fixture.Get<TenantRole.UpdatePermissions>();

    [Fact]
    public void Execute_WithEditableRole_UpdatesPermissions()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Manager")
            .WithDescription("Encargado.")
            .WithIsSystem(true)
            .WithIsEditable(true)
            .WithIsDeletable(false);

        var command = new UpdatePermissionsCommand(
            Groups: ["menu:read", "menu:write"],
            AdditionalScopes: ["res-svc:CancelReservation"],
            ExcludedScopes: ["menu-svc:SetMenuDepositPolicy"]);

        var result = _updatePermissions.Execute(role, command);

        result.Groups.Should().BeEquivalentTo(["menu:read", "menu:write"]);
        result.AdditionalScopes.Should().BeEquivalentTo(["res-svc:CancelReservation"]);
        result.ExcludedScopes.Should().BeEquivalentTo(["menu-svc:SetMenuDepositPolicy"]);
    }

    [Fact]
    public void Execute_WithEmptyPermissions_ClearsAll()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("Manager")
            .WithDescription("Encargado.")
            .WithIsSystem(true)
            .WithIsEditable(true)
            .WithIsDeletable(false)
            .WithGroup("menu:read")
            .WithAdditionalScope("res-svc:CancelReservation")
            .WithExcludedScope("menu-svc:SetMenuDepositPolicy");

        var command = new UpdatePermissionsCommand(
            Groups: [],
            AdditionalScopes: [],
            ExcludedScopes: []);

        var result = _updatePermissions.Execute(role, command);

        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
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

        var command = new UpdatePermissionsCommand(
            Groups: ["menu:read"],
            AdditionalScopes: [],
            ExcludedScopes: []);

        var act = () => _updatePermissions.Execute(role, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*This role cannot be edited*");
    }
}
