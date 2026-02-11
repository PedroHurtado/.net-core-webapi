namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppResendInvitationTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.ResendInvitation _resendInvitation = fixture.Get<ExternalApp.ResendInvitation>();

    [Fact]
    public void Execute_WithPendingInvitation_RemainsPending()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true);

        var result = _resendInvitation.Execute(externalApp);

        result.InvitationStatus.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void Execute_WithCancelledInvitation_TransitionsToPending()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Cancelled)
            .WithIsActive(true);

        var result = _resendInvitation.Execute(externalApp);

        result.InvitationStatus.Should().Be(InvitationStatus.Pending);
    }

    [Fact]
    public void Execute_WhenAlreadyAccepted_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true);

        var act = () => _resendInvitation.Execute(externalApp);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is already accepted*");
    }
}
