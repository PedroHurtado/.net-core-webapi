namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class ActivateExternalAppTests : IClassFixture<DomainFixture>
{
    private readonly Mock<ActivateExternalApp.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivateExternalApp.Service _service;

    public ActivateExternalAppTests(DomainFixture fixture)
    {
        _service = new ActivateExternalApp.Service(
            fixture.Get<ExternalApp.Activate>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveApp_ReturnsResponseWithActiveTrue()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(false);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.IsActive.Should().BeTrue();
    }
}
