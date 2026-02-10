namespace Auth.UnitTests.Sessions.Domain.SessionAggregateTests.Commands.SessionTests;

public class Session_CreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly Session.Create _create = fixture.Get<Session.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsSession()
    {
        var userId = Guid.NewGuid();
        var command = new CreateSessionCommand(UserId: userId);

        var result = _create.Execute(command);

        result.Id.Should().NotBeEmpty();
        result.UserId.Should().Be(userId);
        result.TenantId.Should().BeNull();
        result.RoleId.Should().BeNull();
        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
        result.IsOwner.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        result.LastActivityAt.Should().Be(result.CreatedAt);
        result.ExpiresAt.Should().BeCloseTo(result.CreatedAt.AddDays(30), TimeSpan.FromSeconds(2));
    }
}
