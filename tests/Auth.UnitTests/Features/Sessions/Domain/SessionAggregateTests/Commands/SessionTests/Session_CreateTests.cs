namespace Auth.UnitTests.Sessions.Domain.SessionAggregateTests.Commands.SessionTests;

public class Session_CreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Session.Create _create = fixture.Get<Session.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsSession()
    {
        var userId = Guid.NewGuid();
        var now = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var expiresAt = now.AddDays(30);
        var command = new CreateSessionCommand(UserId: userId, Now: now, ExpiresAt: expiresAt);

        var result = _create.Execute(command);

        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.TenantId.Should().BeNull();
        result.RoleId.Should().BeNull();
        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
        result.IsOwner.Should().BeFalse();
        result.CreatedAt.Should().Be(now);
        result.LastActivityAt.Should().Be(now);
        result.ExpiresAt.Should().Be(expiresAt);
    }
}
