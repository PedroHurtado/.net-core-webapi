namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class ChangeMembershipRoleTests : IClassFixture<DomainFixture>
{
    private readonly Mock<ChangeMembershipRole.IRepository> _repository = new();
    private readonly Mock<IEntityLookup> _entityLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ChangeMembershipRole.Service _service;

    public ChangeMembershipRoleTests(DomainFixture fixture)
    {
        _service = new ChangeMembershipRole.Service(
            fixture.Get<Membership.ChangeRole>(),
            _repository.Object,
            _entityLookup.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ChangesRole()
    {
        var oldRole = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var newRole = new TestableTenantRole(Guid.NewGuid()).WithName("Waiter");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(oldRole)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);
        _entityLookup.Setup(e => e.GetRequiredAsync<TenantRole, Guid>(
            It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(newRole);

        var request = new ChangeMembershipRole.Request(newRole.Id);

        await _service.HandleAsync(entity.Id, request);

        entity.Role.Should().Be(newRole);
    }
}
