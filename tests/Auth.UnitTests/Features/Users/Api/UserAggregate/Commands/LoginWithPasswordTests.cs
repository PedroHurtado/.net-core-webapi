namespace Auth.UnitTests.Features.Users.Api.UserAggregate.Commands;

public class LoginWithPasswordTests : IClassFixture<DomainFixture>
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IMembershipLookup> _membershipLookup = new();
    private readonly Mock<LoginWithPassword.IUserRepository> _userRepository = new();
    private readonly Mock<LoginWithPassword.ISessionRepository> _sessionRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRequestTimestamp> _requestTimestamp = new();
    private readonly LoginWithPassword.Service _service;

    public LoginWithPasswordTests(DomainFixture fixture)
    {
        var now = DateTime.UtcNow;
        _requestTimestamp.Setup(t => t.UtcNow).Returns(now);
        _requestTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));

        _service = new LoginWithPassword.Service(
            fixture.Get<User.RecordLogin>(),
            fixture.Get<Session.Create>(),
            fixture.Get<Session.Refresh>(),
            fixture.Get<Session.SetTenantContext>(),
            _requestTimestamp.Object,
            _passwordHasher.Object,
            _membershipLookup.Object,
            _userRepository.Object,
            _sessionRepository.Object,
            _unitOfWork.Object);
    }

    private static LoginWithPassword.Request CreateValidRequest() => new(
        Email: "admin@fudie.app",
        Password: "SecureP@ss123");

    private TestableUser CreateLocalUser() => new TestableUser(Guid.NewGuid())
        .WithProviderId("local|superadmin")
        .WithProvider(AuthProvider.Local)
        .WithEmail("admin@fudie.app")
        .WithName("Admin")
        .WithPassword(new HashedPassword("hashed-value", "salt-value"))
        .WithIsActive(true);

    // ──────────────────────────────────────────────
    // Guards
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsUnauthorized()
    {
        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync((User?)null);

        var act = () => _service.HandleAsync(CreateValidRequest());

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordInvalid_ThrowsUnauthorized()
    {
        var user = CreateLocalUser();
        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("SecureP@ss123", "hashed-value", "salt-value"))
            .Returns(false);

        var act = () => _service.HandleAsync(CreateValidRequest());

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    // ──────────────────────────────────────────────
    // Happy path
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithValidCredentials_WhenNoSession_ReturnsNewSessionId()
    {
        var user = CreateLocalUser();
        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("SecureP@ss123", "hashed-value", "salt-value"))
            .Returns(true);
        _sessionRepository.Setup(r => r.FindFirstByUserId(user.Id))
            .ReturnsAsync((Session?)null);

        var sessionId = await _service.HandleAsync(CreateValidRequest());

        sessionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_WhenNoSessionAndNoMembership_DoesNotSetTenantContext()
    {
        var user = CreateLocalUser();
        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("SecureP@ss123", "hashed-value", "salt-value"))
            .Returns(true);
        _sessionRepository.Setup(r => r.FindFirstByUserId(user.Id))
            .ReturnsAsync((Session?)null);
        _membershipLookup.Setup(m => m.FindFirstByUserId(user.Id))
            .ReturnsAsync((Membership?)null);

        await _service.HandleAsync(CreateValidRequest());

        _sessionRepository.Verify(r => r.Add(It.Is<Session>(s => s.TenantId == null)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_WhenNoSessionAndHasMembership_SetsTenantContext()
    {
        var user = CreateLocalUser();
        var tenantId = Guid.NewGuid();
        var role = new TestableTenantRole(Guid.NewGuid())
            .WithTenantId(tenantId)
            .WithName("Admin")
            .WithDescription("Admin role")
            .WithIsOwner(false)
            .WithGroup("admin-group")
            .WithAdditionalScope("extra-scope");

        var membership = new TestableMembership(Guid.NewGuid())
            .WithTenantId(tenantId)
            .WithRole(role)
            .WithIsActive(true);

        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("SecureP@ss123", "hashed-value", "salt-value"))
            .Returns(true);
        _sessionRepository.Setup(r => r.FindFirstByUserId(user.Id))
            .ReturnsAsync((Session?)null);
        _membershipLookup.Setup(m => m.FindFirstByUserId(user.Id))
            .ReturnsAsync(membership);

        await _service.HandleAsync(CreateValidRequest());

        _sessionRepository.Verify(r => r.Add(It.Is<Session>(s => s.TenantId == tenantId)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_WhenExistingSession_ReturnsExistingSessionId()
    {
        var user = CreateLocalUser();
        var existingSessionId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var existingSession = new TestableSession(existingSessionId)
            .WithUserId(user.Id)
            .WithCreatedAt(now.AddDays(-5))
            .WithLastActivityAt(now.AddDays(-1))
            .WithExpiresAt(now.AddDays(25));

        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("SecureP@ss123", "hashed-value", "salt-value"))
            .Returns(true);
        _sessionRepository.Setup(r => r.FindFirstByUserId(user.Id))
            .ReturnsAsync(existingSession);

        var sessionId = await _service.HandleAsync(CreateValidRequest());

        sessionId.Should().Be(existingSessionId);
    }

    // ──────────────────────────────────────────────
    // Handler
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Handler_ReturnsNoContent()
    {
        var mockService = new Mock<LoginWithPassword.IService>();
        mockService.Setup(s => s.HandleAsync(It.IsAny<LoginWithPassword.Request>()))
            .ReturnsAsync(Guid.NewGuid());
        var mockCookieService = new Mock<ISessionCookieService>();
        var httpContext = new DefaultHttpContext();
        var request = CreateValidRequest();

        var result = await LoginWithPassword.Handler(mockService.Object, mockCookieService.Object, httpContext, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_CallsSessionCookieService()
    {
        var sessionId = Guid.NewGuid();
        var mockService = new Mock<LoginWithPassword.IService>();
        mockService.Setup(s => s.HandleAsync(It.IsAny<LoginWithPassword.Request>()))
            .ReturnsAsync(sessionId);
        var mockCookieService = new Mock<ISessionCookieService>();
        var httpContext = new DefaultHttpContext();
        var request = CreateValidRequest();

        await LoginWithPassword.Handler(mockService.Object, mockCookieService.Object, httpContext, request);

        mockCookieService.Verify(c => c.Append(httpContext, sessionId), Times.Once);
    }
}
