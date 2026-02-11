namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class DeactivateMembershipTests : IClassFixture<DomainFixture>
{
    private readonly Mock<DeactivateMembership.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeactivateMembership.Service _service;

    public DeactivateMembershipTests(DomainFixture fixture)
    {
        _service = new DeactivateMembership.Service(
            fixture.Get<Membership.Deactivate>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithActiveMembership_ReturnsInactiveResponse()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.IsActive.Should().BeFalse();
    }
}
