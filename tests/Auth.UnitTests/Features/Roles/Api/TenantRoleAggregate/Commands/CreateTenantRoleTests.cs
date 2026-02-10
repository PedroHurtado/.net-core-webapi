namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate.Commands;

public class CreateTenantRoleTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<CreateTenantRole.IRepository> _repository = new();
    private readonly Mock<IQuery> _query = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateTenantRole.Service _service;

    public CreateTenantRoleTests(DomainFixture fixture)
    {
        _service = new CreateTenantRole.Service(
            _tenantId,
            fixture.Get<TenantRole.Create>(),
            _repository.Object,
            _query.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsCreated()
    {
        var expectedResponse = new TenantRoleResponse(
            Guid.NewGuid(), "Admin", "desc", false, true, true, [], [], []);
        var request = new CreateTenantRole.Request("Admin", "desc");
        var mockService = new Mock<CreateTenantRole.IService>();
        mockService.Setup(s => s.HandleAsync(request)).ReturnsAsync(expectedResponse);

        var result = await CreateTenantRole.Handler(mockService.Object, request);

        var created = result as Microsoft.AspNetCore.Http.HttpResults.Created<TenantRoleResponse>;
        created.Should().NotBeNull();
        created!.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        _query.Setup(q => q.Query<TenantRole>())
            .Returns(new List<TenantRole>().AsAsyncQueryable());

        var request = new CreateTenantRole.Request("Admin", "Administrator role");

        var response = await _service.HandleAsync(request);

        response.Name.Should().Be("Admin");
        response.Description.Should().Be("Administrator role");
    }

    [Fact]
    public async Task HandleAsync_WhenNameAlreadyExists_ThrowsConflictException()
    {
        var existing = new TestableTenantRole(Guid.NewGuid())
            .WithName("Admin");
        _query.Setup(q => q.Query<TenantRole>())
            .Returns(new List<TenantRole> { existing }.AsAsyncQueryable());

        var request = new CreateTenantRole.Request("Admin", "desc");

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
