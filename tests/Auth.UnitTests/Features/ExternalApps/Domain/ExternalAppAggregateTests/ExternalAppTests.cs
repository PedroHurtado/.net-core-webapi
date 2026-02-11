namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests;

public class ExternalAppTests
{
    [Fact]
    public void ExternalApp_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var user = new TestableUser(Guid.NewGuid());
        var externalApp = new TestableExternalApp(id);

        // Act
        externalApp
            .WithTenantId(tenantId)
            .WithUser(user)
            .WithName("TPV MiSoftware")
            .WithIsActive(true)
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithApiKeyHash("hash123")
            .WithApiKeyPrefix("fud_a3K9")
            .WithGroup("menu:read")
            .WithAdditionalScope("orders:write")
            .WithExcludedScope("admin:all");

        // Assert
        externalApp.Id.Should().Be(id);
        externalApp.TenantId.Should().Be(tenantId);
        externalApp.User.Should().Be(user);
        externalApp.Name.Should().Be("TPV MiSoftware");
        externalApp.IsActive.Should().BeTrue();
        externalApp.InvitationEmail.Should().Be("dev@misoftware.com");
        externalApp.InvitationStatus.Should().Be(InvitationStatus.Pending);
        externalApp.ApiKeyHash.Should().Be("hash123");
        externalApp.ApiKeyPrefix.Should().Be("fud_a3K9");
        externalApp.Groups.Should().ContainSingle().Which.Should().Be("menu:read");
        externalApp.AdditionalScopes.Should().ContainSingle().Which.Should().Be("orders:write");
        externalApp.ExcludedScopes.Should().ContainSingle().Which.Should().Be("admin:all");
    }

    [Theory]
    [InlineData(InvitationStatus.Pending)]
    [InlineData(InvitationStatus.Accepted)]
    [InlineData(InvitationStatus.Cancelled)]
    public void ExternalApp_WithInvitationStatus_ShouldSetCorrectly(InvitationStatus value)
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithInvitationStatus(value);

        // Assert
        externalApp.InvitationStatus.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExternalApp_IsActive_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithIsActive(value);

        // Assert
        externalApp.IsActive.Should().Be(value);
    }

    [Fact]
    public void Groups_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithGroup("menu:read");

        // Assert
        externalApp.Groups.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        externalApp.Groups.Should().ContainSingle();
    }

    [Fact]
    public void ExternalApp_WithMultipleGroups_ShouldContainAllGroups()
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithGroup("menu:read");
        externalApp.WithGroup("reservation:read");

        // Assert
        externalApp.Groups.Should().HaveCount(2);
        externalApp.Groups.Should().Contain("menu:read");
        externalApp.Groups.Should().Contain("reservation:read");
    }

    [Fact]
    public void AdditionalScopes_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithAdditionalScope("orders:write");

        // Assert
        externalApp.AdditionalScopes.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        externalApp.AdditionalScopes.Should().ContainSingle();
    }

    [Fact]
    public void ExternalApp_WithMultipleAdditionalScopes_ShouldContainAllScopes()
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithAdditionalScope("orders:write");
        externalApp.WithAdditionalScope("menu:write");

        // Assert
        externalApp.AdditionalScopes.Should().HaveCount(2);
        externalApp.AdditionalScopes.Should().Contain("orders:write");
        externalApp.AdditionalScopes.Should().Contain("menu:write");
    }

    [Fact]
    public void ExcludedScopes_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithExcludedScope("admin:all");

        // Assert
        externalApp.ExcludedScopes.Should().BeAssignableTo<IReadOnlyCollection<string>>();
        externalApp.ExcludedScopes.Should().ContainSingle();
    }

    [Fact]
    public void ExternalApp_WithMultipleExcludedScopes_ShouldContainAllScopes()
    {
        // Arrange
        var externalApp = new TestableExternalApp(Guid.NewGuid());

        // Act
        externalApp.WithExcludedScope("admin:all");
        externalApp.WithExcludedScope("billing:write");

        // Assert
        externalApp.ExcludedScopes.Should().HaveCount(2);
        externalApp.ExcludedScopes.Should().Contain("admin:all");
        externalApp.ExcludedScopes.Should().Contain("billing:write");
    }
}
