namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppUpdatePermissionsTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.UpdatePermissions _updatePermissions = fixture.Get<ExternalApp.UpdatePermissions>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesPermissions()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true);

        var command = new UpdateExternalAppPermissionsCommand(
            ["menu:read", "reservation:read"],
            ["orders:write"],
            ["admin:all"]
        );

        var result = _updatePermissions.Execute(externalApp, command);

        result.Groups.Should().HaveCount(2);
        result.Groups.Should().Contain("menu:read");
        result.Groups.Should().Contain("reservation:read");
        result.AdditionalScopes.Should().ContainSingle().Which.Should().Be("orders:write");
        result.ExcludedScopes.Should().ContainSingle().Which.Should().Be("admin:all");
    }

    [Fact]
    public void Execute_WithDuplicates_RemovesDuplicates()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true);

        var command = new UpdateExternalAppPermissionsCommand(
            ["menu:read", "menu:read", "reservation:read"],
            ["orders:write", "orders:write"],
            ["admin:all", "admin:all"]
        );

        var result = _updatePermissions.Execute(externalApp, command);

        result.Groups.Should().HaveCount(2);
        result.AdditionalScopes.Should().HaveCount(1);
        result.ExcludedScopes.Should().HaveCount(1);
    }
}
