namespace Auth.UnitTests.Features.Sessions.Api.SessionAggregate.Commands;

public class ResolveApiKeyTests
{
    private const string TestApiKey = "fud_testkey12345678901234567890ab";
    private const string TestPrefix = "fud_test";
    private static readonly string TestApiKeyHash = BCrypt.Net.BCrypt.HashPassword(TestApiKey);

    private readonly Mock<IQuery> _query = new();
    private readonly Mock<IInternalTokenService> _tokenService = new();
    private readonly ResolveApiKey.Service _service;

    public ResolveApiKeyTests()
    {
        _service = new ResolveApiKey.Service(
            _tokenService.Object,
            _query.Object);
    }

    private TestableExternalApp CreateValidExternalApp()
    {
        var user = new TestableUser(Guid.NewGuid()).WithName("Dev User");

        return new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true)
            .WithApiKeyHash(TestApiKeyHash)
            .WithApiKeyPrefix(TestPrefix)
            .WithUser(user);
    }

    // ──────────────────────────────────────────────
    // Happy path
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithValidApiKey_ReturnsToken()
    {
        var externalApp = CreateValidExternalApp();
        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        var token = await _service.HandleAsync(TestApiKey);

        token.Should().Be("eyJ.test.token");
    }

    [Fact]
    public async Task HandleAsync_WithValidApiKey_PassesCorrectDataToTokenService()
    {
        var externalApp = CreateValidExternalApp();
        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        await _service.HandleAsync(TestApiKey);

        _tokenService.Verify(t => t.GenerateSessionToken(It.Is<SessionTokenData>(d =>
            d.UserId == externalApp.User!.Id &&
            d.TenantId == externalApp.TenantId &&
            d.IsOwner == false)));
    }

    [Fact]
    public async Task HandleAsync_WithValidApiKey_PassesPermissionsToTokenService()
    {
        var groups = new[] { "menu:read", "menu:write" };
        var add = new[] { "orders:read" };
        var exc = new[] { "settings:write" };

        var user = new TestableUser(Guid.NewGuid()).WithName("Dev User");
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true)
            .WithApiKeyHash(TestApiKeyHash)
            .WithApiKeyPrefix(TestPrefix)
            .WithUser(user)
            .WithGroup("menu:read")
            .WithGroup("menu:write")
            .WithAdditionalScope("orders:read")
            .WithExcludedScope("settings:write");

        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        await _service.HandleAsync(TestApiKey);

        _tokenService.Verify(t => t.GenerateSessionToken(It.Is<SessionTokenData>(d =>
            d.Groups.SequenceEqual(new[] { "menu:read", "menu:write" }) &&
            d.AdditionalScopes.SequenceEqual(new[] { "orders:read" }) &&
            d.ExcludedScopes.SequenceEqual(new[] { "settings:write" }))));
    }

    // ──────────────────────────────────────────────
    // API key too short
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithShortApiKey_ThrowsUnauthorizedException()
    {
        var act = () => _service.HandleAsync("short");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid API key");
    }

    // ──────────────────────────────────────────────
    // ExternalApp not found
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithUnknownPrefix_ThrowsUnauthorizedException()
    {
        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Empty<ExternalApp>());

        var act = () => _service.HandleAsync(TestApiKey);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid API key");
    }

    // ──────────────────────────────────────────────
    // Invalid API key (hash mismatch)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithWrongApiKey_ThrowsUnauthorizedException()
    {
        var externalApp = CreateValidExternalApp();
        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));

        var wrongKey = "fud_testwrongkey234567890abcdef12";
        var act = () => _service.HandleAsync(wrongKey);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid API key");
    }

    // ──────────────────────────────────────────────
    // Invitation not accepted
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithPendingInvitation_ThrowsUnauthorizedException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Pending)
            .WithIsActive(true)
            .WithApiKeyHash(TestApiKeyHash)
            .WithApiKeyPrefix(TestPrefix);

        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));

        var act = () => _service.HandleAsync(TestApiKey);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("External app invitation has not been accepted");
    }

    // ──────────────────────────────────────────────
    // Inactive app
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithInactiveApp_ThrowsUnauthorizedException()
    {
        var user = new TestableUser(Guid.NewGuid()).WithName("Dev User");
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(false)
            .WithApiKeyHash(TestApiKeyHash)
            .WithApiKeyPrefix(TestPrefix)
            .WithUser(user);

        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));

        var act = () => _service.HandleAsync(TestApiKey);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("External app is inactive");
    }

    // ──────────────────────────────────────────────
    // User is null
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithNoAssociatedUser_ThrowsUnauthorizedException()
    {
        var externalApp = new TestableExternalApp(Guid.NewGuid())
            .WithTenantId(Guid.NewGuid())
            .WithName("TPV MiSoftware")
            .WithInvitationEmail("dev@misoftware.com")
            .WithInvitationStatus(InvitationStatus.Accepted)
            .WithIsActive(true)
            .WithApiKeyHash(TestApiKeyHash)
            .WithApiKeyPrefix(TestPrefix);

        _query.Setup(q => q.Query<ExternalApp>())
            .Returns(AsyncQueryable.Of<ExternalApp>(externalApp));

        var act = () => _service.HandleAsync(TestApiKey);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("External app has no associated user");
    }
}
