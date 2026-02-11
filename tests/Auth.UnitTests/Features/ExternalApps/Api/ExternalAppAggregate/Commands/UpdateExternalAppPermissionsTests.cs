namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class UpdateExternalAppPermissionsTests : IClassFixture<DomainFixture>
{
    private readonly Mock<UpdateExternalAppPermissions.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateExternalAppPermissions.Service _service;

    public UpdateExternalAppPermissionsTests(DomainFixture fixture)
    {
        _service = new UpdateExternalAppPermissions.Service(
            fixture.Get<ExternalApp.UpdatePermissions>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesPermissions()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var request = new UpdateExternalAppPermissions.Request(
            Groups: ["menu:read", "reservation:read"],
            AdditionalScopes: ["billing:write"],
            ExcludedScopes: ["admin:delete"]);

        await _service.HandleAsync(entity.Id, request);

        entity.Groups.Should().HaveCount(2);
        entity.Groups.Should().Contain("menu:read");
        entity.Groups.Should().Contain("reservation:read");
        entity.AdditionalScopes.Single().Should().Be("billing:write");
        entity.ExcludedScopes.Single().Should().Be("admin:delete");
    }
}
