namespace Auth.UnitTests.Features.Users.Domain.UserAggregateTests.CommandsTests;

public class HashedPasswordCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly HashedPassword.Create _create = fixture.Get<HashedPassword.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsHashedPassword()
    {
        var command = new CreateHashedPasswordCommand("SecureP@ss123");

        var result = _create.Execute(command);

        result.Hash.Should().NotBeNullOrEmpty();
        result.Salt.Should().NotBeNullOrEmpty();
    }
}
