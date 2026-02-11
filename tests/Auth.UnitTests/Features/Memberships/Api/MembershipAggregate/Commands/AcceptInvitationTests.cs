namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class AcceptInvitationTests : IClassFixture<DomainFixture>
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<AcceptInvitation.IRepository> _repository = new();
    private readonly Mock<IEntityLookup> _entityLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AcceptInvitation.Service _service;

    public AcceptInvitationTests(DomainFixture fixture)
    {
        _service = new AcceptInvitation.Service(
            new CurrentUserId(_userId),
            fixture.Get<Membership.AcceptInvitation>(),
            _repository.Object,
            _entityLookup.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithPendingInvitation_ReturnsAcceptedResponse()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var user = new TestableUser(_userId).WithName("María García").WithPhone("+34666777888");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);
        _entityLookup.Setup(e => e.GetRequiredAsync<User, Guid>(
            It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(user);

        var response = await _service.HandleAsync(entity.Id);

        response.InvitationStatus.Should().Be("Accepted");
        response.UserId.Should().Be(_userId);
        response.UserName.Should().Be("María García");
    }
}
