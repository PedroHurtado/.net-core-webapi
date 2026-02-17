namespace Auth.UnitTests.Sessions.Domain.SessionAggregateTests.Commands.SessionTests;

public class Session_ClearTenantContextTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Session.ClearTenantContext _clearTenantContext = fixture.Get<Session.ClearTenantContext>();

    [Fact]
    public void Execute_WithTenantContext_ClearsAllTenantData()
    {
        var now = DateTime.UtcNow;
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(Guid.NewGuid())
            .WithGroups(["menu:read"])
            .WithAdditionalScopes(["reservation-service:CancelReservation"])
            .WithExcludedScopes(["menu-service:SetMenuDepositPolicy"])
            .WithIsOwner(true)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        var result = _clearTenantContext.Execute(session);

        result.TenantId.Should().BeNull();
        result.RoleId.Should().BeNull();
        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
        result.IsOwner.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithoutTenantContext_RemainsCleared()
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

        var result = _clearTenantContext.Execute(session);

        result.TenantId.Should().BeNull();
        result.RoleId.Should().BeNull();
        result.Groups.Should().BeEmpty();
        result.IsOwner.Should().BeFalse();
    }
}
