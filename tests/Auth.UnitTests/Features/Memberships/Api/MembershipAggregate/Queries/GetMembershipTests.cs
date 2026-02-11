namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Queries;

public class GetMembershipTests
{
    private readonly Mock<GetMembership.IRepository> _repository = new();
    private readonly GetMembership.Service _service;

    public GetMembershipTests()
    {
        _service = new GetMembership.Service(_repository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingMembership_ReturnsResponse()
    {
        var user = new TestableUser(Guid.NewGuid()).WithName("María García").WithPhone("+34666777888");
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");
        var entity = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithUser(user)
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("maria@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        _repository.Setup(r => r.Get(entity.Id)).ReturnsAsync(entity);

        var response = await _service.HandleAsync(entity.Id);

        response.Id.Should().Be(entity.Id);
        response.UserId.Should().Be(user.Id);
        response.UserName.Should().Be("María García");
        response.UserPhone.Should().Be("+34666777888");
        response.RoleId.Should().Be(role.Id);
        response.RoleName.Should().Be("Manager");
        response.IsActive.Should().BeTrue();
        response.InvitationEmail.Should().Be("maria@ejemplo.com");
        response.InvitationStatus.Should().Be("Accepted");
    }
}
