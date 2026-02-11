namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class CancelExternalAppInvitationTests : IClassFixture<DomainFixture>
{
    private readonly Mock<CancelExternalAppInvitation.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CancelExternalAppInvitation.Service _service;

    public CancelExternalAppInvitationTests(DomainFixture fixture)
    {
        _service = new CancelExternalAppInvitation.Service(
            fixture.Get<ExternalApp.CancelInvitation>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithPendingInvitation_ReturnsResponseWithCancelledStatus()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.InvitationStatus.Should().Be("Cancelled");
    }
}
