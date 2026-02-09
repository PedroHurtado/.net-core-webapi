namespace Auth.UnitTests.Features.Users.Domain.UserAggregateTests;

public class UserTests
{
    [Fact]
    public void User_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var password = new HashedPassword("$2a$12$abc123", "random-salt");

        // Act
        var user = new TestableUser(id)
            .WithProviderId("local|superadmin-001")
            .WithProvider(AuthProvider.Local)
            .WithEmail("admin@fudie.app")
            .WithName("Fudie Admin")
            .WithPhone("+34900000000")
            .WithAvatarUrl(null)
            .WithPassword(password)
            .WithLastLoginAt(DateTime.UtcNow)
            .WithIsActive(true);

        // Assert
        user.Id.Should().Be(id);
        user.ProviderId.Should().Be("local|superadmin-001");
        user.Provider.Should().Be(AuthProvider.Local);
        user.Email.Should().Be("admin@fudie.app");
        user.Name.Should().Be("Fudie Admin");
        user.Phone.Should().Be("+34900000000");
        user.AvatarUrl.Should().BeNull();
        user.Password.Should().Be(password);
        user.LastLoginAt.Should().NotBeNull();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsOAuth_WithGoogleProvider_ShouldReturnTrue()
    {
        // Arrange
        var user = new TestableUser(Guid.NewGuid());

        // Act
        user.WithProvider(AuthProvider.Google);

        // Assert
        user.IsOAuth.Should().BeTrue();
    }

    [Fact]
    public void IsOAuth_WithLocalProvider_ShouldReturnFalse()
    {
        // Arrange
        var user = new TestableUser(Guid.NewGuid());

        // Act
        user.WithProvider(AuthProvider.Local);

        // Assert
        user.IsOAuth.Should().BeFalse();
    }

    [Fact]
    public void HasPassword_WithPassword_ShouldReturnTrue()
    {
        // Arrange
        var user = new TestableUser(Guid.NewGuid());

        // Act
        user.WithPassword(new HashedPassword("$2a$12$abc123", "random-salt"));

        // Assert
        user.HasPassword.Should().BeTrue();
    }

    [Fact]
    public void HasPassword_WithoutPassword_ShouldReturnFalse()
    {
        // Arrange
        var user = new TestableUser(Guid.NewGuid());

        // Act
        user.WithPassword(null);

        // Assert
        user.HasPassword.Should().BeFalse();
    }

    [Theory]
    [InlineData(AuthProvider.Google)]
    [InlineData(AuthProvider.Local)]
    public void User_WithProvider_ShouldSetCorrectly(AuthProvider value)
    {
        // Arrange
        var user = new TestableUser(Guid.NewGuid());

        // Act
        user.WithProvider(value);

        // Assert
        user.Provider.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void User_IsActive_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var user = new TestableUser(Guid.NewGuid());

        // Act
        user.WithIsActive(value);

        // Assert
        user.IsActive.Should().Be(value);
    }
}
