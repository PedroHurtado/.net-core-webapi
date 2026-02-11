namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class DeactivateExternalAppTests : IClassFixture<DomainFixture>
{
    private readonly Mock<DeactivateExternalApp.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeactivateExternalApp.Service _service;

    public DeactivateExternalAppTests(DomainFixture fixture)
    {
        _service = new DeactivateExternalApp.Service(
            fixture.Get<ExternalApp.Deactivate>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithActiveApp_ReturnsResponseWithInactiveFalse()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.IsActive.Should().BeFalse();
    }
}
