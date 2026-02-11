namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class ResendExternalAppInvitationTests : IClassFixture<DomainFixture>
{
    private readonly Mock<ResendExternalAppInvitation.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ResendExternalAppInvitation.Service _service;

    public ResendExternalAppInvitationTests(DomainFixture fixture)
    {
        _service = new ResendExternalAppInvitation.Service(
            fixture.Get<ExternalApp.ResendInvitation>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithCancelledInvitation_ResendsSuccessfully()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Cancelled);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        await _service.HandleAsync(entity.Id);

        entity.InvitationStatus.Should().Be(InvitationStatus.Pending);
    }
}
