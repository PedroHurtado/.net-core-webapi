namespace Auth.UnitTests.Sessions.Domain.SessionAggregateTests.Commands.SessionTests;

public class Session_RefreshTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Session.Refresh _refresh = fixture.Get<Session.Refresh>();

    [Fact]
    public void Execute_WithActiveSession_UpdatesLastActivityAndExpires()
    {
        var createdAt = DateTime.UtcNow.AddDays(-5);
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

        var result = _refresh.Execute(session);

        result.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(2));
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

        var act = () => _refresh.Execute(session);

        act.Should().Throw<UnauthorizedException>()
            .WithMessage("*Session expired*");
    }
}
