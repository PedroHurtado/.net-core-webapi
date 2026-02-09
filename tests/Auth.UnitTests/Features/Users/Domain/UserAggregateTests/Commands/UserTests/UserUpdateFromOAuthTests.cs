namespace Auth.UnitTests.Users.Domain.UserAggregateTests.Commands.UserTests;

public class UserUpdateFromOAuthTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly User.UpdateFromOAuth _updateFromOAuth = fixture.Get<User.UpdateFromOAuth>();

    [Fact]
    public void Execute_WithValidOAuthData_UpdatesUser()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("old@test.com")
            .WithName("Old Name")
            .WithAvatarUrl("https://old-photo.jpg")
            .WithIsActive(true);

        var command = new UpdateFromOAuthCommand(
            Email: "new@test.com",
            Name: "New Name",
            AvatarUrl: "https://new-photo.jpg");

        var result = _updateFromOAuth.Execute(user, command);

        result.Email.Should().Be("new@test.com");
        result.Name.Should().Be("New Name");
        result.AvatarUrl.Should().Be("https://new-photo.jpg");
        result.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Execute_WithNullAvatarUrl_UpdatesUser()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("old@test.com")
            .WithName("Old Name")
            .WithAvatarUrl("https://old-photo.jpg")
            .WithIsActive(true);

        var command = new UpdateFromOAuthCommand(
            Email: "new@test.com",
            Name: "New Name",
            AvatarUrl: null);

        var result = _updateFromOAuth.Execute(user, command);

        result.AvatarUrl.Should().BeNull();
        result.Email.Should().Be("new@test.com");
        result.Name.Should().Be("New Name");
        result.LastLoginAt.Should().NotBeNull();
    }
}
