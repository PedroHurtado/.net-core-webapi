namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate.Queries;

public class GetTenantRolesTests
{
    private readonly Mock<IQuery> _query = new();
    private readonly GetTenantRoles.Service _service;

    public GetTenantRolesTests()
    {
        _service = new GetTenantRoles.Service(_query.Object);
    }

    [Fact]
    public async Task Handler_ReturnsOkWithResponse()
    {
        var expectedResponse = new List<TenantRoleResponse>();
        var mockService = new Mock<GetTenantRoles.IService>();
        mockService.Setup(s => s.HandleAsync()).ReturnsAsync(expectedResponse);

        var result = await GetTenantRoles.Handler(mockService.Object);

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<List<TenantRoleResponse>>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeSameAs(expectedResponse);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRoles_ReturnsOrderedList()
    {
        var role1 = new TestableTenantRole(Guid.NewGuid())
            .WithName("Admin")
            .WithDescription("Admin role");
        var role2 = new TestableTenantRole(Guid.NewGuid())
            .WithName("Basic")
            .WithDescription("Basic role");
        _query.Setup(q => q.Query<TenantRole>())
            .Returns(new List<TenantRole> { role2, role1 }.AsAsyncQueryable());

        var response = await _service.HandleAsync();

        response.Should().HaveCount(2);
        response.First().Name.Should().Be("Admin");
        response.Last().Name.Should().Be("Basic");
    }
}
