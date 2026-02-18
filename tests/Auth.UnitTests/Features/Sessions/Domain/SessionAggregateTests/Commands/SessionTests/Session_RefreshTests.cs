namespace Auth.UnitTests.Sessions.Domain.SessionAggregateTests.Commands.SessionTests;

public class Session_RefreshTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Session.Refresh _refresh = fixture.Get<Session.Refresh>();

    [Fact]
    public void Execute_WithActiveSession_UpdatesLastActivityAndExpires()
    {
        var now = DateTime.UtcNow;
        var createdAt = now.AddDays(-5);
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(createdAt)
            .WithLastActivityAt(createdAt)
            .WithExpiresAt(createdAt.AddDays(30));

        var expiresAt = now.AddDays(30);
        var command = new RefreshSessionCommand(Now: now, ExpiresAt: expiresAt);

        var result = _refresh.Execute(session, command);

        result.LastActivityAt.Should().Be(now);
        result.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void Execute_WithExpiredSession_ThrowsUnauthorizedException()
    {
        var createdAt = DateTime.UtcNow.AddDays(-60);
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithTenantId(null)
            .WithRoleId(null)
            .WithGroups([])
            .WithAdditionalScopes([])
            .WithExcludedScopes([])
            .WithIsOwner(false)
            .WithCreatedAt(createdAt)
            .WithLastActivityAt(createdAt.AddDays(29))
            .WithExpiresAt(createdAt.AddDays(30));

        var now = DateTime.UtcNow;
        var command = new RefreshSessionCommand(Now: now, ExpiresAt: now.AddDays(30));

        var act = () => _refresh.Execute(session, command);

        act.Should().Throw<UnauthorizedException>()
            .WithMessage("*Session expired*");
    }
}
