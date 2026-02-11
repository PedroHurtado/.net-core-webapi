namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate;

public class ExternalAppResponseTests
{
    #region ExternalAppResponse.Map

    [Fact]
    public void ExternalAppResponse_Map_MapsAllProperties()
    {
        var user = new TestableUser(Guid.NewGuid())
            .WithName("Dev User");
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithUser(user)
            .WithName("TPV MiSoftware")
            .WithIsActive(true)
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithApiKeyPrefix("fud_a3K9")
            .WithApiKeyExpiresAt(new DateTime(2026, 12, 31))
            .WithGroup("menu:read")
            .WithGroup("reservation:read")
            .WithAdditionalScope("billing:write")
            .WithExcludedScope("admin:delete");

        var response = ExternalAppResponse.Map(externalApp);

        response.Id.Should().Be(externalApp.Id);
        response.TenantId.Should().Be(externalApp.TenantId);
        response.UserId.Should().Be(user.Id);
        response.Name.Should().Be("TPV MiSoftware");
        response.IsActive.Should().BeTrue();
        response.InvitationEmail.Should().Be("dev@misoftware.com");
        response.InvitationStatus.Should().Be("Accepted");
        response.ApiKeyPrefix.Should().Be("fud_a3K9");
        response.ApiKeyExpiresAt.Should().Be(new DateTime(2026, 12, 31));
        response.Groups.Should().HaveCount(2);
        response.Groups.Should().Contain("menu:read");
        response.Groups.Should().Contain("reservation:read");
        response.AdditionalScopes.Should().HaveCount(1);
        response.AdditionalScopes.Single().Should().Be("billing:write");
        response.ExcludedScopes.Should().HaveCount(1);
        response.ExcludedScopes.Single().Should().Be("admin:delete");
    }

    [Fact]
    public void ExternalAppResponse_Map_WithNullUser_MapsNullUserId()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithIsActive(true)
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var response = ExternalAppResponse.Map(externalApp);

        response.UserId.Should().BeNull();
        response.ApiKeyPrefix.Should().BeNull();
        response.ApiKeyExpiresAt.Should().BeNull();
        response.InvitationStatus.Should().Be("Pending");
    }

    [Fact]
    public void ExternalAppResponse_Map_WithEmptyCollections_MapsEmptyCollections()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithIsActive(true)
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        var response = ExternalAppResponse.Map(externalApp);

        response.Groups.Should().BeEmpty();
        response.AdditionalScopes.Should().BeEmpty();
        response.ExcludedScopes.Should().BeEmpty();
    }

    #endregion
}