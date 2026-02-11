namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class ActivateMembershipTests : IClassFixture<DomainFixture>
{
    private readonly Mock<ActivateMembership.IRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ActivateMembership.Service _service;

    public ActivateMembershipTests(DomainFixture fixture)
    {
        _service = new ActivateMembership.Service(
            fixture.Get<Membership.Activate>(),
            _repository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveMembership_ReturnsActiveResponse()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(false)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.IsActive.Should().BeTrue();
    }
}
