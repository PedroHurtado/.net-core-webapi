namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class UpdateExternalAppTests : IClassFixture<DomainFixture>
{
    private readonly Mock<UpdateExternalApp.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateExternalApp.Service _service;

    public UpdateExternalAppTests(DomainFixture fixture)
    {
        _service = new UpdateExternalApp.Service(
            fixture.Get<ExternalApp.Update>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesName()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var request = new UpdateExternalApp.Request("TPV NuevoNombre");

        await _service.HandleAsync(entity.Id, request);

        entity.Name.Should().Be("TPV NuevoNombre");
    }
}
