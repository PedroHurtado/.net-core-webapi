namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppCancelInvitationTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.CancelInvitation _cancelInvitation = fixture.Get<ExternalApp.CancelInvitation>();

    [Fact]
    public void Execute_WithPendingInvitation_CancelsInvitation()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true);

        var result = _cancelInvitation.Execute(externalApp);

        result.InvitationStatus.Should().Be(InvitationStatus.Cancelled);
    }

    [Fact]
    public void Execute_WhenNotPending_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true);

        var act = () => _cancelInvitation.Execute(externalApp);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }
}
