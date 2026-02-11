namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppDeactivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.Deactivate _deactivate = fixture.Get<ExternalApp.Deactivate>();

    [Fact]
    public void Execute_WithActiveExternalApp_Deactivates()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true);

        var result = _deactivate.Execute(externalApp);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(false);

        var act = () => _deactivate.Execute(externalApp);

        act.Should().Throw<ConflictException>()
            .WithMessage("*External app is already inactive*");
    }
}
