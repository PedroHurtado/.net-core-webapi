namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class DeleteMembershipTests
{
    private readonly Mock<DeleteMembership.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteMembership.Service _service;

    public DeleteMembershipTests()
    {
        _service = new DeleteMembership.Service(
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingMembership_Completes()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        await _service.HandleAsync(entity.Id);
    }
}
