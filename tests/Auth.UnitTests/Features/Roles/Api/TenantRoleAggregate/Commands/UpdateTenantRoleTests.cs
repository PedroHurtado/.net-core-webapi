namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate.Commands;

public class UpdateTenantRoleTests : IClassFixture<DomainFixture>
{
    private readonly Mock<UpdateTenantRole.IRepository> _repository = new();
    private readonly Mock<IQuery> _query = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateTenantRole.Service _service;

    public UpdateTenantRoleTests(DomainFixture fixture)
    {
        _service = new UpdateTenantRole.Service(
            fixture.Get<TenantRole.Update>(),
            _repository.Object,
            _query.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContent()
    {
        var mockService = new Mock<UpdateTenantRole.IService>();
        var id = Guid.NewGuid();
        var request = new UpdateTenantRole.Request("Admin", "desc");
        mockService.Setup(s => s.HandleAsync(id, request)).Returns(Task.CompletedTask);

        var result = await UpdateTenantRole.Handler(mockService.Object, id, request);

        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesRole()
    {
        var id = Guid.NewGuid();
        var role = new TestableTenantRole(id)
            .WithTenantId(Guid.NewGuid())
            .WithName("OldName")
            .WithDescription("Old description")
            ;
        _repository.Setup(r => r.Get(id)).ReturnsAsync(role);
        _query.Setup(q => q.Query<TenantRole>())
            .Returns(new List<TenantRole>().AsAsyncQueryable());

        var request = new UpdateTenantRole.Request("NewName", "New description");

        await _service.HandleAsync(id, request);

        role.Name.Should().Be("NewName");
        role.Description.Should().Be("New description");
    }

    [Fact]
    public async Task HandleAsync_WhenNameAlreadyExists_ThrowsConflictException()
    {
        var id = Guid.NewGuid();
        var role = new TestableTenantRole(id)
            .WithName("OldName")
            .WithDescription("desc");
        _repository.Setup(r => r.Get(id)).ReturnsAsync(role);

        var other = new TestableTenantRole(Guid.NewGuid())
            .WithName("NewName");
        _query.Setup(q => q.Query<TenantRole>())
            .Returns(new List<TenantRole> { other }.AsAsyncQueryable());

        var request = new UpdateTenantRole.Request("NewName", "desc");

        var act = () => _service.HandleAsync(id, request);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
