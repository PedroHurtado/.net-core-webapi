namespace Auth.UnitTests.Features.Sessions.Domain.SessionAggregateTests;

public class SessionTests
{
    [Fact]
    public void Session_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var groups = new[] { "menu:read", "menu:write" };
        var additionalScopes = new[] { "reservation-service:CancelReservation" };
        var excludedScopes = new[] { "menu-service:SetMenuDepositPolicy" };

        // Act
        var session = new TestableSession(id)
            .WithUserId(userId)
            .WithTenantId(tenantId)
            .WithRoleId(roleId)
            .WithGroups(groups)
            .WithAdditionalScopes(additionalScopes)
            .WithExcludedScopes(excludedScopes)
            .WithIsOwner(false)
            .WithCreatedAt(now)
            .WithLastActivityAt(now)
            .WithExpiresAt(now.AddDays(30));

        // Assert
        session.Id.Should().Be(id);
        session.UserId.Should().Be(userId);
        session.TenantId.Should().Be(tenantId);
        session.RoleId.Should().Be(roleId);
        session.Groups.Should().BeEquivalentTo(groups);
        session.AdditionalScopes.Should().BeEquivalentTo(additionalScopes);
        session.ExcludedScopes.Should().BeEquivalentTo(excludedScopes);
        session.IsOwner.Should().BeFalse();
        session.CreatedAt.Should().Be(now);
        session.LastActivityAt.Should().Be(now);
        session.ExpiresAt.Should().Be(now.AddDays(30));
    }

    [Fact]
    public void IsExpired_WithPastExpiresAt_ShouldReturnTrue()
    {
        // Arrange
        var session = new TestableSession(Guid.NewGuid());

        // Act
        session.WithExpiresAt(DateTimeOffset.UtcNow.AddDays(-1));

        // Assert
        session.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithFutureExpiresAt_ShouldReturnFalse()
    {
        // Arrange
        var session = new TestableSession(Guid.NewGuid());

        // Act
        session.WithExpiresAt(DateTimeOffset.UtcNow.AddDays(30));

        // Assert
        session.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void HasTenantContext_WithTenantId_ShouldReturnTrue()
    {
        // Arrange
        var session = new TestableSession(Guid.NewGuid());

        // Act
        session.WithTenantId(Guid.NewGuid());

        // Assert
        session.HasTenantContext.Should().BeTrue();
    }

    [Fact]
    public void HasTenantContext_WithoutTenantId_ShouldReturnFalse()
    {
        // Arrange
        var session = new TestableSession(Guid.NewGuid());

        // Act
        session.WithTenantId(null);

        // Assert
        session.HasTenantContext.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Session_IsOwner_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var session = new TestableSession(Guid.NewGuid());

        // Act
        session.WithIsOwner(value);

        // Assert
        session.IsOwner.Should().Be(value);
    }
}