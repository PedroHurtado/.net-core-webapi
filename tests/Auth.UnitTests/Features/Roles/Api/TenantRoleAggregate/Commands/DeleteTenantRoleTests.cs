namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate.Commands;

public class DeleteTenantRoleTests
{
    private readonly Mock<DeleteTenantRole.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteTenantRole.Service _service;

    public DeleteTenantRoleTests()
    {
        _service = new DeleteTenantRole.Service(
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handler_WithValidId_ReturnsNoContent()
    {
        var mockService = new Mock<DeleteTenantRole.IService>();
        var id = Guid.NewGuid();
        mockService.Setup(s => s.HandleAsync(id)).Returns(Task.CompletedTask);

        var result = await DeleteTenantRole.Handler(mockService.Object, id);

        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
    }

    [Fact]
    public async Task HandleAsync_WhenRoleIsNotOwner_Succeeds()
    {
        var id = Guid.NewGuid();
        var role = new TestableTenantRole(id)
            .WithName("Custom")
            .WithDescription("Custom role")
            ;
        _repository.Setup(r => r.Get(id)).ReturnsAsync(role);

        var act = () => _service.HandleAsync(id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenRoleIsOwner_ThrowsConflictException()
    {
        var id = Guid.NewGuid();
        var role = new TestableTenantRole(id)
            .WithName("System")
            .WithDescription("System role")
            .WithIsOwner(true);
        _repository.Setup(r => r.Get(id)).ReturnsAsync(role);

        var act = () => _service.HandleAsync(id);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
