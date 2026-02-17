namespace Auth.UnitTests.Sessions.Domain.SessionAggregateTests.Commands.SessionTests;

public class Session_SetTenantContextTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Session.SetTenantContext _setTenantContext = fixture.Get<Session.SetTenantContext>();

    [Fact]
    public void Execute_WithValidCommand_SetsTenantContext()
    {
        var now = DateTime.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var command = new SetTenantContextCommand(
            TenantId: tenantId,
            RoleId: roleId,
            Groups: ["menu:read"],
            AdditionalScopes: [],
            ExcludedScopes: [],
            IsOwner: false);

        var result = _setTenantContext.Execute(session, command);

        result.TenantId.Should().Be(tenantId);
        result.RoleId.Should().Be(roleId);
        result.Groups.Should().BeEquivalentTo(["menu:read"]);
        result.IsOwner.Should().BeFalse();
    }

    [Fact]
    public void Execute_AsOwner_SetsIsOwnerTrue()
    {
        var now = DateTime.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var command = new SetTenantContextCommand(
            TenantId: Guid.NewGuid(),
            RoleId: Guid.NewGuid(),
            Groups: [],
            AdditionalScopes: [],
            ExcludedScopes: [],
            IsOwner: true);

        var result = _setTenantContext.Execute(session, command);

        result.IsOwner.Should().BeTrue();
        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithExistingTenant_ChangesToNewTenant()
    {
        var now = DateTime.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(Guid.NewGuid())
            .WithGroups(["menu:read"])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var newTenantId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();
        var command = new SetTenantContextCommand(
            TenantId: newTenantId,
            RoleId: newRoleId,
            Groups: ["reservation:read"],
            AdditionalScopes: [],
            ExcludedScopes: [],
            IsOwner: false);

        var result = _setTenantContext.Execute(session, command);

        result.TenantId.Should().Be(newTenantId);
        result.RoleId.Should().Be(newRoleId);
        result.Groups.Should().BeEquivalentTo(["reservation:read"]);
    }
}
