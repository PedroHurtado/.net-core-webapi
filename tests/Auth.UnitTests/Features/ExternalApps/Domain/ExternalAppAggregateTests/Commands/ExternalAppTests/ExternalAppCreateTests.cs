namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.Create _create = fixture.Get<ExternalApp.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsExternalApp()
    {
        var tenantId = Guid.NewGuid();
        var command = new CreateExternalAppCommand(tenantId, "TPV MiSoftware", "dev@misoftware.com");

        var result = _create.Execute(command);

        result.Id.Should().NotBeEmpty();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("TPV MiSoftware");
        result.InvitationEmail.Should().Be("dev@misoftware.com");
        result.InvitationStatus.Should().Be(InvitationStatus.Pending);
        result.User.Should().BeNull();
        result.IsActive.Should().BeTrue();
        result.ApiKeyHash.Should().BeNull();
        result.ApiKeyPrefix.Should().BeNull();
        result.ApiKeyExpiresAt.Should().BeNull();
    }
}
