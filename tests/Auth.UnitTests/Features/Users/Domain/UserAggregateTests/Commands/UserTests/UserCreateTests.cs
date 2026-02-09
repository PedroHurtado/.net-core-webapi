namespace Auth.UnitTests.Users.Domain.UserAggregateTests.Commands.UserTests;

public class UserCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly User.Create _create = fixture.Get<User.Create>();

    [Fact]
    public void Execute_WithValidGoogleCommand_ReturnsUser()
    {
        var command = new CreateUserCommand(
            ProviderId: "google|123",
            Provider: AuthProvider.Google,
            Email: "pedro@test.com",
            Name: "Pedro",
            AvatarUrl: "https://lh3.googleusercontent.com/photo.jpg");

        var result = _create.Execute(command);

        result.ProviderId.Should().Be("google|123");
        result.Provider.Should().Be(AuthProvider.Google);
        result.Email.Should().Be("pedro@test.com");
        result.Name.Should().Be("Pedro");
        result.AvatarUrl.Should().Be("https://lh3.googleusercontent.com/photo.jpg");
        result.Password.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Execute_WithValidLocalCommand_ReturnsUser()
    {
        var command = new CreateUserCommand(
            ProviderId: "local|superadmin-001",
            Provider: AuthProvider.Local,
            Email: "admin@fudie.app",
            Name: "Fudie Admin",
            PlainPassword: "SecureP@ss123");

        var result = _create.Execute(command);

        result.ProviderId.Should().Be("local|superadmin-001");
        result.Provider.Should().Be(AuthProvider.Local);
        result.Password.Should().NotBeNull();
        result.Password!.Hash.Should().NotBeEmpty();
        result.Password!.Salt.Should().NotBeEmpty();
        result.IsActive.Should().BeTrue();
        result.LastLoginAt.Should().BeNull();
    }
}
