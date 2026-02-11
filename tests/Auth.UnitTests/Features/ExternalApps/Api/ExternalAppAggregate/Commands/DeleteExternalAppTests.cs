namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class DeleteExternalAppTests
{
    private readonly Mock<DeleteExternalApp.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteExternalApp.Service _service;

    public DeleteExternalAppTests()
    {
        _service = new DeleteExternalApp.Service(
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingApp_DeletesSuccessfully()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com");

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        await _service.HandleAsync(entity.Id);

        _repository.Verify(r => r.Remove(entity), Times.Once);
    }
}
