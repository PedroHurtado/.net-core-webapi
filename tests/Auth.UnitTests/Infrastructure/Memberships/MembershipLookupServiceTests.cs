namespace Auth.UnitTests.Infrastructure.Memberships;

public class MembershipLookupServiceTests
{
    private readonly Mock<IQuery> _query = new();
    private readonly MembershipLookupService _sut;

    public MembershipLookupServiceTests()
    {
        _sut = new MembershipLookupService(_query.Object);
    }

    // ──────────────────────────────────────────────
    // FindFirstByUserId
    // ──────────────────────────────────────────────

    [Fact]
    public async Task FindFirstByUserId_WhenMembershipExists_ReturnsMembership()
    {
        var userId = Guid.NewGuid();
        var membership = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { membership }.AsAsyncQueryable());

        var result = await _sut.FindFirstByUserId(userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(membership.Id);
    }

    [Fact]
    public async Task FindFirstByUserId_WhenNoMembership_ReturnsNull()
    {
        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership>().AsAsyncQueryable());

        var result = await _sut.FindFirstByUserId(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindFirstByUserId_WhenMultipleMemberships_ReturnsFirst()
    {
        var userId = Guid.NewGuid();
        var membership1 = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);
        var membership2 = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { membership1, membership2 }.AsAsyncQueryable());

        var result = await _sut.FindFirstByUserId(userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(membership1.Id);
    }

    // ──────────────────────────────────────────────
    // ExistsByUserIdAndTenantId
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExistsByUserIdAndTenantId_WhenActiveMembershipExists_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var membership = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(tenantId)
            .WithIsActive(true);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { membership }.AsAsyncQueryable());

        var result = await _sut.ExistsByUserIdAndTenantId(userId, tenantId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserIdAndTenantId_WhenNoMembership_ReturnsFalse()
    {
        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership>().AsAsyncQueryable());

        var result = await _sut.ExistsByUserIdAndTenantId(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByUserIdAndTenantId_WhenInactiveMembership_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var membership = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(tenantId)
            .WithIsActive(false);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { membership }.AsAsyncQueryable());

        var result = await _sut.ExistsByUserIdAndTenantId(userId, tenantId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByUserIdAndTenantId_WhenDifferentTenant_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var membership = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { membership }.AsAsyncQueryable());

        var result = await _sut.ExistsByUserIdAndTenantId(userId, Guid.NewGuid());

        result.Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // FindAllByUserId
    // ──────────────────────────────────────────────

    [Fact]
    public async Task FindAllByUserId_WhenMembershipsExist_ReturnsAll()
    {
        var userId = Guid.NewGuid();
        var membership1 = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);
        var membership2 = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { membership1, membership2 }.AsAsyncQueryable());

        var result = await _sut.FindAllByUserId(userId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAllByUserId_WhenNoMemberships_ReturnsEmptyList()
    {
        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership>().AsAsyncQueryable());

        var result = await _sut.FindAllByUserId(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindAllByUserId_DoesNotReturnMembershipsOfOtherUsers()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var mine = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(userId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);
        var other = new TestableMembership(Guid.NewGuid())
            .WithUser(new TestableUser(otherUserId))
            .WithTenantId(Guid.NewGuid())
            .WithIsActive(true);

        _query.Setup(q => q.Query<Membership>())
            .Returns(new List<Membership> { mine, other }.AsAsyncQueryable());

        var result = await _sut.FindAllByUserId(userId);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(mine.Id);
    }
}
