namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppUpdateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.Update _update = fixture.Get<ExternalApp.Update>();

    [Fact]
    public void Execute_WithValidCommand_UpdatesName()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true);

        var command = new UpdateExternalAppCommand("TPV NuevoNombre");

        var result = _update.Execute(externalApp, command);

        result.Name.Should().Be("TPV NuevoNombre");
    }
}
