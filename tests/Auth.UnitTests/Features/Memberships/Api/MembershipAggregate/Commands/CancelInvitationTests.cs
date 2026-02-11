namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class CancelInvitationTests : IClassFixture<DomainFixture>
{
    private readonly Mock<CancelInvitation.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CancelInvitation.Service _service;

    public CancelInvitationTests(DomainFixture fixture)
    {
        _service = new CancelInvitation.Service(
            fixture.Get<Membership.CancelInvitation>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithPendingInvitation_ReturnsCancelledResponse()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.InvitationStatus.Should().Be("Cancelled");
    }
}
