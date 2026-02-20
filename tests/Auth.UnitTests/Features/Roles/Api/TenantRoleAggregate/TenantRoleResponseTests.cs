namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate;

public class TenantRoleResponseTests
{
    #region TenantRoleResponse.Map

    [Fact]
    public void TenantRoleResponse_Map_MapsAllProperties()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithName("Admin")
            .WithDescription("Administrator role")
            .WithIsOwner(true)
            .WithGroup("users")
            .WithAdditionalScope("read:all")
            .WithExcludedScope("delete:all");

        var response = TenantRoleResponse.Map(role);

        response.Id.Should().Be(role.Id);
        response.Name.Should().Be("Admin");
        response.Description.Should().Be("Administrator role");
        response.IsOwner.Should().BeTrue();
        response.Groups.Should().HaveCount(1);
        response.Groups.First().Should().Be("users");
        response.AdditionalScopes.Should().HaveCount(1);
        response.AdditionalScopes.First().Should().Be("read:all");
        response.ExcludedScopes.Should().HaveCount(1);
        response.ExcludedScopes.First().Should().Be("delete:all");
    }

    [Fact]
    public void TenantRoleResponse_Map_WithEmptyCollections_MapsEmptyCollections()
    {
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithName("Basic")
            .WithDescription("Basic role");

        var response = TenantRoleResponse.Map(role);

        response.Groups.Should().BeEmpty();
        response.AdditionalScopes.Should().BeEmpty();
        response.ExcludedScopes.Should().BeEmpty();
    }

    #endregion
}
