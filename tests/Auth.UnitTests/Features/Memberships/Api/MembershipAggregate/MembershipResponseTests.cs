namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate;

public class MembershipResponseTests
{
    #region MembershipResponse.Map

    [Fact]
    public void MembershipResponse_Map_MapsAllProperties()
    {
        var user = new TestableUser(Guid.NewGuid());
        var role = new TestableTenantRole(Guid.NewGuid());
        var membership = new TestableMembership(Guid.NewGuid())
            .WithUser(user)
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var response = MembershipResponse.Map(membership);

        response.Id.Should().Be(membership.Id);
        response.UserId.Should().Be(user.Id);
        response.RoleId.Should().Be(role.Id);
        response.IsActive.Should().BeTrue();
        response.InvitationEmail.Should().Be("maria@ejemplo.com");
        response.InvitationStatus.Should().Be("Accepted");
    }

    [Fact]
    public void MembershipResponse_Map_WithNullUser_MapsNullUserId()
    {
        var role = new TestableTenantRole(Guid.NewGuid());
        var membership = new TestableMembership(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var response = MembershipResponse.Map(membership);

        response.UserId.Should().BeNull();
        response.InvitationStatus.Should().Be("Pending");
    }

    #endregion
}
