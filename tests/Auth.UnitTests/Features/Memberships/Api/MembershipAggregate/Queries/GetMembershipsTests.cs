namespace Auth.UnitTests.Features.Memberships.Api.MembershipAggregate.Queries;

public class GetMembershipsTests
{
    private readonly Mock<IQuery> _query = new();
    private readonly GetMemberships.Service _service;

    public GetMembershipsTests()
    {
        _service = new GetMemberships.Service(_query.Object);
    }

    [Fact]
    public async Task HandleAsync_WithMemberships_ReturnsOrderedList()
    {
        var user = new TestableUser(Guid.NewGuid()).WithName("María García").WithPhone("+34666777888");
        var role = new TestableTenantRole(Guid.NewGuid()).WithName("Manager");

        var membership1 = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithUser(user)
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("beta@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Accepted);

        var membership2 = new TestableMembership(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithRole(role)
            .WithIsActive(true)
            .WithInvitationEmail("alpha@ejemplo.com")
            .WithInvitationStatus(InvitationStatus.Pending);

        _query.Setup(q => q.Query<Membership>())
            .Returns(AsyncQueryable.Of(membership1, membership2));

        var response = await _service.HandleAsync();

        response.Should().HaveCount(2);
        response[0].InvitationEmail.Should().Be("alpha@ejemplo.com");
        response[1].InvitationEmail.Should().Be("beta@ejemplo.com");
        response[1].UserId.Should().Be(user.Id);
        response[1].UserName.Should().Be("María García");
        response[1].RoleName.Should().Be("Manager");
    }

    [Fact]
    public async Task HandleAsync_WithNoMemberships_ReturnsEmptyList()
    {
        _query.Setup(q => q.Query<Membership>())
            .Returns(AsyncQueryable.Empty<Membership>());

        var response = await _service.HandleAsync();

        response.Should().BeEmpty();
    }
}
