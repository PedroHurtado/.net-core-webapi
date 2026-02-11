namespace Auth.UnitTests.Features.ExternalApps.Api.ExternalAppAggregate.Queries;

public class GetExternalAppTests
{
    private readonly Mock<GetExternalApp.IRepository> _repository = new();
    private readonly GetExternalApp.Service _service;

    public GetExternalAppTests()
    {
        _service = new GetExternalApp.Service(_repository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingApp_ReturnsResponse()
    {
        var entity = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithIsActive(true)
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.Id.Should().Be(entity.Id);
        response.Name.Should().Be("TPV MiSoftware");
        response.InvitationEmail.Should().Be("dev@misoftware.com");
        response.IsActive.Should().BeTrue();
        response.InvitationStatus.Should().Be("Pending");
    }
}
