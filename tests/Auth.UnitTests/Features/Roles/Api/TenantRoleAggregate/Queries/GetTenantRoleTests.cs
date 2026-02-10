namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate.Queries;

public class GetTenantRoleTests
{
    private readonly Mock<GetTenantRole.IRepository> _repository = new();
    private readonly GetTenantRole.Service _service;

    public GetTenantRoleTests()
    {
        _service = new GetTenantRole.Service(_repository.Object);
    }

    [Fact]
    public async Task Handler_WithValidId_ReturnsOk()
    {
        var expectedResponse = new TenantRoleResponse(
            Guid.NewGuid(), "Admin", "desc", false, true, true, [], [], []);
        var mockService = new Mock<GetTenantRole.IService>();
        var id = Guid.NewGuid();
        mockService.Setup(s => s.HandleAsync(id)).ReturnsAsync(expectedResponse);

        var result = await GetTenantRole.Handler(mockService.Object, id);

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<TenantRoleResponse>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRole_ReturnsResponse()
    {
        var id = Guid.NewGuid();
        var role = new TestableTenantRole(id)
            .WithName("Admin")
            .WithDescription("Administrator role")
            .WithIsSystem(true)
            .WithIsEditable(false)
            .WithIsDeletable(false);
        _repository.Setup(r => r.Get(id)).ReturnsAsync(role);

        var response = await _service.HandleAsync(id);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Admin");
        response.Description.Should().Be("Administrator role");
        response.IsSystem.Should().BeTrue();
    }
}
