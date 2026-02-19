namespace Auth.UnitTests.Features.Sessions.Api.Commands;

public class ResolveAuthTests : IClassFixture<DomainFixture>
{
    private readonly Mock<ResolveAuth.IRepository> _repository = new();
    private readonly Mock<ResolveAuth.IUserRepository> _userRepository = new();
    private readonly Mock<IMembershipLookup> _membershipLookup = new();
    private readonly Mock<IInternalTokenService> _tokenService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRequestTimestamp> _requestTimestamp = new();
    private readonly ResolveAuth.Service _service;

    public ResolveAuthTests(DomainFixture fixture)
    {
        var now = DateTime.UtcNow;
        _requestTimestamp.Setup(t => t.UtcNow).Returns(now);
        _requestTimestamp.Setup(t => t.ExpiresAt).Returns(now.AddDays(30));

        _service = new ResolveAuth.Service(
            fixture.Get<Session.Refresh>(),
            _requestTimestamp.Object,
            _tokenService.Object,
            _membershipLookup.Object,
            _repository.Object,
            _userRepository.Object,
            _unitOfWork.Object);
    }

    private TestableSession CreateValidSession() => new TestableSession(Guid.NewGuid())
        .WithUserId(Guid.NewGuid())
        .WithCreatedAt(DateTime.UtcNow.AddHours(-1))
        .WithLastActivityAt(DateTime.UtcNow.AddHours(-1))
        .WithExpiresAt(DateTime.UtcNow.AddDays(29));

    // ──────────────────────────────────────────────
    // Happy path — sesión sin tenant
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithValidSessionWithoutTenant_ReturnsToken()
    {
        var session = CreateValidSession();
        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(true);
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        var token = await _service.HandleAsync(session.Id);

        token.Should().Be("eyJ.test.token");
    }

    [Fact]
    public async Task HandleAsync_WithValidSession_RefreshesSession()
    {
        var session = CreateValidSession();
        var previousLastActivity = session.LastActivityAt;
        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(true);
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        await _service.HandleAsync(session.Id);

        session.LastActivityAt.Should().BeAfter(previousLastActivity);
    }

    [Fact]
    public async Task HandleAsync_WithValidSession_PassesCorrectDataToTokenService()
    {
        var session = CreateValidSession();
        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(true);
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        await _service.HandleAsync(session.Id);

        _tokenService.Verify(t => t.GenerateSessionToken(It.Is<SessionTokenData>(d =>
            d.UserId == session.UserId &&
            d.TenantId == null &&
            d.IsOwner == false)));
    }

    // ──────────────────────────────────────────────
    // Happy path — sesión con tenant (owner)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithOwnerSession_PassesOwnerDataToTokenService()
    {
        var session = CreateValidSession()
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(Guid.NewGuid())
            .WithIsOwner(true)
            .WithGroups([]);

        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(true);
        _membershipLookup.Setup(m => m.ExistsByUserIdAndTenantId(session.UserId, session.TenantId!.Value))
            .ReturnsAsync(true);
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        await _service.HandleAsync(session.Id);

        _tokenService.Verify(t => t.GenerateSessionToken(It.Is<SessionTokenData>(d =>
            d.TenantId == session.TenantId &&
            d.IsOwner == true)));
    }

    // ──────────────────────────────────────────────
    // Happy path — sesión con tenant (permisos)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithPermissionsSession_PassesPermissionsToTokenService()
    {
        var groups = new[] { "menu:read", "menu:write" };
        var add = new[] { "reservation-service:CancelReservation" };
        var exc = new[] { "menu-service:SetMenuDepositPolicy" };

        var session = CreateValidSession()
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(Guid.NewGuid())
            .WithIsOwner(false)
            .WithGroups(groups)
            .WithAdditionalScopes(add)
            .WithExcludedScopes(exc);

        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(true);
        _membershipLookup.Setup(m => m.ExistsByUserIdAndTenantId(session.UserId, session.TenantId!.Value))
            .ReturnsAsync(true);
        _tokenService.Setup(t => t.GenerateSessionToken(It.IsAny<SessionTokenData>()))
            .Returns("eyJ.test.token");

        await _service.HandleAsync(session.Id);

        _tokenService.Verify(t => t.GenerateSessionToken(It.Is<SessionTokenData>(d =>
            d.Groups == groups &&
            d.AdditionalScopes == add &&
            d.ExcludedScopes == exc)));
    }

    // ──────────────────────────────────────────────
    // Sesión no encontrada
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithNonExistentSession_ThrowsUnauthorizedException()
    {
        var sessionId = Guid.NewGuid();
        _repository.Setup(r => r.FindFirstById(sessionId)).ReturnsAsync((Session?)null);

        var act = () => _service.HandleAsync(sessionId);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid session");
    }

    // ──────────────────────────────────────────────
    // Sesión expirada
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithExpiredSession_ThrowsUnauthorizedExceptionAndDeletesSession()
    {
        var session = new TestableSession(Guid.NewGuid())
            .WithUserId(Guid.NewGuid())
            .WithCreatedAt(DateTime.UtcNow.AddDays(-31))
            .WithLastActivityAt(DateTime.UtcNow.AddDays(-31))
            .WithExpiresAt(DateTime.UtcNow.AddDays(-1));

        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _repository.Setup(r => r.DeleteById(session.Id)).ReturnsAsync(1);

        var act = () => _service.HandleAsync(session.Id);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Session expired");
        _repository.Verify(r => r.DeleteById(session.Id), Times.Once);
    }

    // ──────────────────────────────────────────────
    // Usuario inactivo
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ThrowsUnauthorizedException()
    {
        var session = CreateValidSession();
        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(false);

        var act = () => _service.HandleAsync(session.Id);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is inactive");
    }

    // ──────────────────────────────────────────────
    // Membership inactiva
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithInactiveMembership_ThrowsUnauthorizedException()
    {
        var session = CreateValidSession()
            .WithTenantId(Guid.NewGuid())
            .WithRoleId(Guid.NewGuid());

        _repository.Setup(r => r.FindFirstById(session.Id)).ReturnsAsync(session);
        _userRepository.Setup(r => r.ExistsByIdAndIsActiveTrue(session.UserId)).ReturnsAsync(true);
        _membershipLookup.Setup(m => m.ExistsByUserIdAndTenantId(session.UserId, session.TenantId!.Value))
            .ReturnsAsync(false);

        var act = () => _service.HandleAsync(session.Id);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Membership is inactive");
    }
}
