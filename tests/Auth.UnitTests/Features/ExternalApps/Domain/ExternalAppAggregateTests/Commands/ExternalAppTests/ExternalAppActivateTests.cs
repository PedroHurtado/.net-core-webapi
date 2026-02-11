namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppActivateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.Activate _activate = fixture.Get<ExternalApp.Activate>();

    [Fact]
    public void Execute_WithInactiveExternalApp_Activates()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(false);

        var result = _activate.Execute(externalApp);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true);

        var act = () => _activate.Execute(externalApp);

        act.Should().Throw<ConflictException>()
            .WithMessage("*External app is already active*");
    }
}
