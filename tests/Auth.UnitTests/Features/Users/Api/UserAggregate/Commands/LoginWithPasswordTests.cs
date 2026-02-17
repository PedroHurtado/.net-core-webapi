namespace Auth.UnitTests.Features.Users.Api.UserAggregate.Commands;

public class LoginWithPasswordTests : IClassFixture<DomainFixture>
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<LoginWithPassword.IUserRepository> _userRepository = new();
    private readonly Mock<LoginWithPassword.ISessionRepository> _sessionRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly LoginWithPassword.Service _service;

    public LoginWithPasswordTests(DomainFixture fixture)
    {
        _service = new LoginWithPassword.Service(
            fixture.Get<User.RecordLogin>(),
            fixture.Get<Session.Create>(),
            _passwordHasher.Object,
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
    public async Task HandleAsync_WithValidCredentials_WhenExistingSession_ReturnsExistingSessionId()
    {
        var user = CreateLocalUser();
        var existingSessionId = Guid.NewGuid();
        _userRepository.Setup(r => r.FindFirstByEmailAndProvider("admin@fudie.app", AuthProvider.Local))
            .ReturnsAsync(user);
        _passwordHasher.Setup(p => p.Verify("SecureP@ss123", "hashed-value", "salt-value"))
            .Returns(true);
        _sessionRepository.Setup(r => r.FindFirstByUserId(user.Id))
            .ReturnsAsync(new TestableSession(existingSessionId));

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
        var httpContext = new DefaultHttpContext();
        var request = CreateValidRequest();

        var result = await LoginWithPassword.Handler(mockService.Object, httpContext, request);

        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Handler_SetsFudieSessionCookie()
    {
        var sessionId = Guid.NewGuid();
        var mockService = new Mock<LoginWithPassword.IService>();
        mockService.Setup(s => s.HandleAsync(It.IsAny<LoginWithPassword.Request>()))
            .ReturnsAsync(sessionId);
        var httpContext = new DefaultHttpContext();
        var request = CreateValidRequest();

        await LoginWithPassword.Handler(mockService.Object, httpContext, request);

        httpContext.Response.Headers.SetCookie.ToString().Should().Contain("fudie_session");
        httpContext.Response.Headers.SetCookie.ToString().Should().Contain(sessionId.ToString());
    }
}
