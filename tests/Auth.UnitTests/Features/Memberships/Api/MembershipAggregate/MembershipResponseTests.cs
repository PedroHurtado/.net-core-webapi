namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate;

public class MembershipResponseTests
{
    #region MembershipResponse.Map

    [Fact]
    public void MembershipResponse_Map_MapsAllProperties()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithName("María García")
            .WithPhone("+34666777888");
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithName("Manager");
        var membership = new TestableMembership(Guid.NewGuid())
            .WithUser(user)
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var response = MembershipResponse.Map(membership);

        response.Id.Should().Be(membership.Id);
        response.UserId.Should().Be(user.Id);
        response.UserName.Should().Be("María García");
        response.UserPhone.Should().Be("+34666777888");
        response.RoleId.Should().Be(role.Id);
        response.RoleName.Should().Be("Manager");
        response.IsActive.Should().BeTrue();
        response.InvitationEmail.Should().Be("maria@ejemplo.com");
        response.InvitationStatus.Should().Be("Accepted");
    }

    [Fact]
    public void MembershipResponse_Map_WithNullUser_MapsNullUserFields()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithName("Waiter");
        var membership = new TestableMembership(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var response = MembershipResponse.Map(membership);

        response.UserId.Should().BeNull();
        response.UserName.Should().BeNull();
        response.UserPhone.Should().BeNull();
        response.RoleName.Should().Be("Waiter");
        response.InvitationStatus.Should().Be("Pending");
    }

    #endregion
}
