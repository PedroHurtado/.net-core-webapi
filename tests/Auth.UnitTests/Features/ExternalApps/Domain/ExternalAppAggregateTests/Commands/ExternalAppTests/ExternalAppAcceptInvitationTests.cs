namespace Auth.UnitTests.Features.ExternalApps.Domain.ExternalAppAggregateTests.Commands.ExternalAppTests;

public class ExternalAppAcceptInvitationTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly ExternalApp.AcceptInvitation _acceptInvitation = fixture.Get<ExternalApp.AcceptInvitation>();

    [Fact]
    public void Execute_WithValidCommand_AcceptsInvitation()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true);

        var user = new TestableUser(Guid.NewGuid());
        var command = new AcceptExternalAppInvitationCommand(user, "hash123", "fud_a3K9");

        var result = _acceptInvitation.Execute(externalApp, command);

        result.InvitationStatus.Should().Be(InvitationStatus.Accepted);
        result.User.Should().Be(user);
        result.ApiKeyHash.Should().Be("hash123");
        result.ApiKeyPrefix.Should().Be("fud_a3K9");
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

        var command = new AcceptExternalAppInvitationCommand(new TestableUser(Guid.NewGuid()), "hash", "fud_pre1");

        var act = () => _acceptInvitation.Execute(externalApp, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }

    [Fact]
    public void Execute_WhenCancelled_ThrowsConflictException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Cancelled)
            .WithIsActive(true);

        var command = new AcceptExternalAppInvitationCommand(new TestableUser(Guid.NewGuid()), "hash", "fud_pre1");

        var act = () => _acceptInvitation.Execute(externalApp, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Invitation is not pending*");
    }
}
