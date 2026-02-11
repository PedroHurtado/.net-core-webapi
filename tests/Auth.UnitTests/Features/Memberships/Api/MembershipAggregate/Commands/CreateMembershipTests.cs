namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Commands;

public class CreateMembershipTests : IClassFixture<DomainFixture>
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Mock<CreateMembership.IRepository> _repository = new();
    private readonly Mock<IEntityLookup> _entityLookup = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateMembership.Service _service;

    public CreateMembershipTests(DomainFixture fixture)
    {
        _service = new CreateMembership.Service(
            _tenantId,
            fixture.Get<Membership.Create>(),
            _repository.Object,
            _entityLookup.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsResponse()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        _entityLookup.Setup(e => e.GetRequiredAsync<TenantRole, Guid>(
            It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(role);
        _repository.Setup(r => r.ExistsByInvitationEmail(It.IsAny<string>())).ReturnsAsync(false);

        var request = new CreateMembership.Request("maria@ejemplo.com", role.Id);

        var response = await _service.HandleAsync(request);

        response.InvitationEmail.Should().Be("maria@ejemplo.com");
        response.RoleId.Should().Be(role.Id);
        response.RoleName.Should().Be("Manager");
        response.InvitationStatus.Should().Be("Pending");
        response.IsActive.Should().BeTrue();
        response.UserId.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenEmailExists_ThrowsConflictException()
    {
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        _entityLookup.Setup(e => e.GetRequiredAsync<TenantRole, Guid>(
            It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(role);
        _repository.Setup(r => r.ExistsByInvitationEmail("maria@ejemplo.com")).ReturnsAsync(true);

        var request = new CreateMembership.Request("maria@ejemplo.com", role.Id);

        var act = () => _service.HandleAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
