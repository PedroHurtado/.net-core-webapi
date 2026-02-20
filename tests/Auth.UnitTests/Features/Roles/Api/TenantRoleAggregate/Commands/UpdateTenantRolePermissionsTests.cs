namespace Auth.UnitTests.Features.Roles.Api.TenantRoleAggregate.Commands;

public class UpdateTenantRolePermissionsTests : IClassFixture<DomainFixture>
{
    private readonly Mock<UpdateTenantRolePermissions.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateTenantRolePermissions.Service _service;

    public UpdateTenantRolePermissionsTests(DomainFixture fixture)
    {
        _service = new UpdateTenantRolePermissions.Service(
            fixture.Get<TenantRole.UpdatePermissions>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handler_WithValidRequest_ReturnsNoContent()
    {
        var mockService = new Mock<UpdateTenantRolePermissions.IService>();
        var id = Guid.NewGuid();
        var request = new UpdateTenantRolePermissions.Request(["users"], ["read:all"], ["delete:all"]);
        mockService.Setup(s => s.HandleAsync(id, request)).Returns(Task.CompletedTask);

        var result = await UpdateTenantRolePermissions.Handler(mockService.Object, id, request);

        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.NoContent>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_UpdatesPermissions()
    {
        var id = Guid.NewGuid();
        var role = new TestableTenantRole(id)
            .WithTenantId(Guid.NewGuid())
            .WithName("Admin")
            .WithDescription("Admin role")
            ;
        _repository.Setup(r => r.Get(id)).ReturnsAsync(role);

        var request = new UpdateTenantRolePermissions.Request(
            ["users", "billing"],
            ["read:all"],
            ["delete:all"]);

        await _service.HandleAsync(id, request);

        role.Groups.Should().BeEquivalentTo(new[] { "users", "billing" });
        role.AdditionalScopes.Should().BeEquivalentTo(new[] { "read:all" });
        role.ExcludedScopes.Should().BeEquivalentTo(new[] { "delete:all" });
    }
}
