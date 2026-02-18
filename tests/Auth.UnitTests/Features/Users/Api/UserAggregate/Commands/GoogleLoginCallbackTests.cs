namespace Auth.UnitTests.Features.Users.Api.UserAggregate.Commands;

public class GoogleLoginCallbackTests : IClassFixture<DomainFixture>
{
    private readonly Mock<IGoogleOAuthSettings> _googleOAuthSettings = new();
    private readonly Mock<IGoogleOAuthApi> _googleOAuthApi = new();
    private readonly Mock<IGoogleIdTokenValidator> _idTokenValidator = new();
    private readonly Mock<GoogleLoginCallback.IRepository> _repository = new();
    private readonly Mock<GoogleLoginCallback.ISessionRepository> _sessionRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRequestTimestamp> _requestTimestamp = new();
    private readonly GoogleLoginCallback.Service _service;

    public GoogleLoginCallbackTests(DomainFixture fixture)
    {
        var now = DateTime.UtcNow;
        _requestTimestamp.Setup(t => t.UtcNow).Returns(now);
        _requestTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));

        _googleOAuthSettings.Setup(s => s.Get()).Returns(new GoogleOAuthSettings(
            ClientId: "test-client-id",
            ClientSecret: "test-secret",
            RedirectUri: "http://localhost/callback",
            AuthUri: "https://accounts.google.com/o/oauth2/auth",
            TokenUri: "https://oauth2.googleapis.com/token",
            CertsUri: "https://certs.example.com"));

        _service = new GoogleLoginCallback.Service(
            _googleOAuthSettings.Object,
            _googleOAuthApi.Object,
            _idTokenValidator.Object,
            fixture.Get<User.Create>(),
            fixture.Get<User.UpdateFromOAuth>(),
            fixture.Get<Session.Create>(),
            fixture.Get<Session.Refresh>(),
            _requestTimestamp.Object,
            _repository.Object,
            _sessionRepository.Object,
            _unitOfWork.Object);
    }

    private void SetupOAuthMocks(
        string sub = "sub123",
        string email = "pedro@test.com",
        string name = "Pedro",
        string? picture = "https://photo.jpg")
    {
        _googleOAuthApi.Setup(a => a.ExchangeCodeAsync(It.IsAny<GoogleTokenRequest>()))
            .ReturnsAsync(new GoogleTokenResponse("id-token", "access-token", "Bearer", 3600));

        _idTokenValidator.Setup(v => v.ValidateAsync("id-token"))
            .ReturnsAsync(new GoogleIdTokenClaims(sub, email, name, picture));
    }

    [Fact]
    public async Task HandleAsync_WithNewUser_ReturnsSessionId()
    {
        SetupOAuthMocks();
        _repository.Setup(r => r.FindFirstByProviderIdAndProvider("google|sub123", AuthProvider.Google))
            .ReturnsAsync((User?)null);
        _sessionRepository.Setup(r => r.FindFirstByUserId(It.IsAny<Guid>()))
            .ReturnsAsync((Session?)null);

        var sessionId = await _service.HandleAsync("auth-code");

        sessionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_UpdatesUserAndReturnsSessionId()
    {
        SetupOAuthMocks();
        var existingUser = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|sub123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("old@test.com")
            .WithName("Old Name")
            .WithIsActive(true);

        _repository.Setup(r => r.FindFirstByProviderIdAndProvider("google|sub123", AuthProvider.Google))
            .ReturnsAsync(existingUser);
        _sessionRepository.Setup(r => r.FindFirstByUserId(existingUser.Id))
            .ReturnsAsync((Session?)null);

        var sessionId = await _service.HandleAsync("auth-code");

        sessionId.Should().NotBeEmpty();
        existingUser.Email.Should().Be("pedro@test.com");
        existingUser.Name.Should().Be("Pedro");
        existingUser.AvatarUrl.Should().Be("https://photo.jpg");
    }

    [Fact]
    public async Task HandleAsync_WithExistingUserAndSession_ReturnsExistingSessionId()
    {
        SetupOAuthMocks();
        var existingSessionId = Guid.NewGuid();
        var existingUser = new TestableUser(Guid.NewGuid())
            .WithProviderId("google|sub123")
            .WithProvider(AuthProvider.Google)
            .WithEmail("old@test.com")
            .WithName("Old Name")
            .WithIsActive(true);

        var now = DateTime.UtcNow;
        var existingSession = new TestableSession(existingSessionId)
            .WithUserId(existingUser.Id)
            .WithCreatedAt(now.AddDays(-5))
            .WithLastActivityAt(now.AddDays(-1))
            .WithExpiresAt(now.AddDays(25));

        _repository.Setup(r => r.FindFirstByProviderIdAndProvider("google|sub123", AuthProvider.Google))
            .ReturnsAsync(existingUser);
        _sessionRepository.Setup(r => r.FindFirstByUserId(existingUser.Id))
            .ReturnsAsync(existingSession);

        var sessionId = await _service.HandleAsync("auth-code");

        sessionId.Should().Be(existingSessionId);
    }

    [Fact]
    public async Task Handler_WhenCookieStateIsNull_ReturnsUnauthorized()
    {
        var mockService = new Mock<GoogleLoginCallback.IService>();
        var mockCookieService = new Mock<ISessionCookieService>();
        var mockOAuthSettings = new Mock<IOAuthSettings>();
        mockOAuthSettings.Setup(s => s.Get()).Returns(new OAuthSettings("fudie_oauth_state", 300));
        var httpContext = new DefaultHttpContext();

        var result = await GoogleLoginCallback.Handler(
            mockService.Object, mockCookieService.Object, mockOAuthSettings.Object, httpContext, "code", "state");

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task Handler_WhenCookieStateMismatch_ReturnsUnauthorized()
    {
        var mockService = new Mock<GoogleLoginCallback.IService>();
        var mockCookieService = new Mock<ISessionCookieService>();
        var mockOAuthSettings = new Mock<IOAuthSettings>();
        mockOAuthSettings.Setup(s => s.Get()).Returns(new OAuthSettings("fudie_oauth_state", 300));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "fudie_oauth_state=different-state";

        var result = await GoogleLoginCallback.Handler(
            mockService.Object, mockCookieService.Object, mockOAuthSettings.Object, httpContext, "code", "expected-state");

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task Handler_WhenStateMatches_CallsCookieServiceAndRedirects()
    {
        var sessionId = Guid.NewGuid();
        var mockService = new Mock<GoogleLoginCallback.IService>();
        mockService.Setup(s => s.HandleAsync("auth-code")).ReturnsAsync(sessionId);
        var mockCookieService = new Mock<ISessionCookieService>();
        var mockOAuthSettings = new Mock<IOAuthSettings>();
        mockOAuthSettings.Setup(s => s.Get()).Returns(new OAuthSettings("fudie_oauth_state", 300));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "fudie_oauth_state=valid-state";

        var result = await GoogleLoginCallback.Handler(
            mockService.Object, mockCookieService.Object, mockOAuthSettings.Object, httpContext, "auth-code", "valid-state");

        result.Should().BeOfType<RedirectHttpResult>()
            .Which.Url.Should().Be("/");
        mockCookieService.Verify(c => c.Append(httpContext, sessionId), Times.Once);
    }
}
