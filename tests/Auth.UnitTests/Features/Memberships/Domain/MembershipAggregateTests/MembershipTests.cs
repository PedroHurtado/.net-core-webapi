namespace Auth.UnitTests.Features.Memberships.Domain.MembershipAggregateTests;

public class MembershipTests
{
    [Fact]
    public void Membership_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var role = new TestableTenantRole(Guid.NewGuid());
        var user = new TestableUser(Guid.NewGuid());
        var membership = new TestableMembership(id);

        // Act
        membership
            .WithTenantId(tenantId)
            .WithUser(user)
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        // Assert
        membership.Id.Should().Be(id);
        membership.TenantId.Should().Be(tenantId);
        membership.User.Should().Be(user);
        membership.Role.Should().Be(role);
        membership.IsActive.Should().BeTrue();
        membership.InvitationEmail.Should().Be("maria@ejemplo.com");
        membership.InvitationStatus.Should().Be(InvitationStatus.Pending);
    }

    [Theory]
    [InlineData(InvitationStatus.Pending)]
    [InlineData(InvitationStatus.Accepted)]
    [InlineData(InvitationStatus.Cancelled)]
    public void Membership_WithInvitationStatus_ShouldSetCorrectly(InvitationStatus value)
    {
        // Arrange
        var membership = new TestableMembership(Guid.NewGuid());

        // Act
        membership.WithInvitationStatus(value);

        // Assert
        membership.InvitationStatus.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Membership_IsActive_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var membership = new TestableMembership(Guid.NewGuid());

        // Act
        membership.WithIsActive(value);

        // Assert
        membership.IsActive.Should().Be(value);
    }
}
