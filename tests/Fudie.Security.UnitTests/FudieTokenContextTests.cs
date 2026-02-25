namespace Fudie.Security.UnitTests;

public class FudieTokenContextTests
{
    [Fact]
    public void ShouldCreateWithAllProperties()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var groups = new[] { "menu:read", "menu:write" };
        var additionalScopes = new[] { "admin:manage" };
        var excludedScopes = new[] { "billing:delete" };

        var context = new FudieTokenContext(userId, tenantId, false, groups, additionalScopes, excludedScopes);

        context.UserId.Should().Be(userId);
        context.TenantId.Should().Be(tenantId);
        context.IsOwner.Should().BeFalse();
        context.Groups.Should().BeEquivalentTo(groups);
        context.AdditionalScopes.Should().BeEquivalentTo(additionalScopes);
        context.ExcludedScopes.Should().BeEquivalentTo(excludedScopes);
    }

    [Fact]
    public void ShouldAllowNullTenantId()
    {
        var context = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        context.TenantId.Should().BeNull();
    }

    [Fact]
    public void ShouldSupportOwnerFlag()
    {
        var context = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), true, [], [], []);

        context.IsOwner.Should().BeTrue();
    }

}
