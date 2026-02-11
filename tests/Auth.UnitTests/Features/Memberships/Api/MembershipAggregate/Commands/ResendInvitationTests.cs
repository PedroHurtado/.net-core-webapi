namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class ResendInvitationTests : IClassFixture<DomainFixture>
{
    private readonly Mock<ResendInvitation.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ResendInvitation.Service _service;

    public ResendInvitationTests(DomainFixture fixture)
    {
        _service = new ResendInvitation.Service(
            fixture.Get<Membership.ResendInvitation>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithPendingInvitation_Completes()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        await _service.HandleAsync(entity.Id);
    }
}
