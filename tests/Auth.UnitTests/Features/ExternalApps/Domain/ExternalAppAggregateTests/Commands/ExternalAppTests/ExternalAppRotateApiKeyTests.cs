namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppRotateApiKeyTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.RotateApiKey _rotateApiKey = fixture.Get<ExternalApp.RotateApiKey>();

    [Fact]
    public void Execute_WithAcceptedAndActive_RotatesApiKey()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true)
            .WithApiKeyHash("oldHash")
            .WithApiKeyPrefix("fud_old1");

        var command = new RotateExternalAppApiKeyCommand("newHash", "newSalt", "fud_new1");

        var result = _rotateApiKey.Execute(externalApp, command);

        result.ApiKeyHash.Should().Be("newHash");
        result.ApiKeySalt.Should().Be("newSalt");
        result.ApiKeyPrefix.Should().Be("fud_new1");
    }

    [Fact]
    public void Execute_WhenNotAccepted_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true);

        var command = new RotateExternalAppApiKeyCommand("hash", "salt", "fud_pre1");

        var act = () => _rotateApiKey.Execute(externalApp, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*External app invitation has not been accepted*");
    }

    [Fact]
    public void Execute_WhenInactive_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(false);

        var command = new RotateExternalAppApiKeyCommand("hash", "salt", "fud_pre1");

        var act = () => _rotateApiKey.Execute(externalApp, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Cannot rotate key for inactive external app*");
    }
}
