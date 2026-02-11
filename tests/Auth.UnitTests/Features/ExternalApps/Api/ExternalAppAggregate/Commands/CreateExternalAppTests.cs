namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class CreateExternalAppTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<CreateExternalApp.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateExternalApp.Service _service;

    public CreateExternalAppTests(DomainFixture fixture)
    {
        _service = new CreateExternalApp.Service(
            _tenantId,
            fixture.Get<ExternalApp.Create>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var request = new CreateExternalApp.Request("TPV MiSoftware", "dev@misoftware.com");

        var response = await _service.HandleAsync(request);

        response.TenantId.Should().Be(_tenantId);
        response.Name.Should().Be("TPV MiSoftware");
        response.InvitationEmail.Should().Be("dev@misoftware.com");
        response.InvitationStatus.Should().Be("Pending");
        response.UserId.Should().BeNull();
        response.IsActive.Should().BeTrue();
        response.ApiKeyPrefix.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenEmailDuplicate_ThrowsConflictException()
    {
        _repository.Setup(r => r.ExistsByInvitationEmail("dev@misoftware.com")).ReturnsAsync(true);

        var request = new CreateExternalApp.Request("TPV MiSoftware", "dev@misoftware.com");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
