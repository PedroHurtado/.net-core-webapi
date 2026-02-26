namespace Auth.UnitTests.Infrastructure.Jwt;

public class InternalTokenServiceTests
{
    private readonly ECDsa _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly string _kid = "test-kid";
    private readonly Mock<IJwtKeyProvider> _keyProvider = new();
    private readonly InternalTokenService _sut;

    public InternalTokenServiceTests()
    {
        _keyProvider.Setup(k => k.GetPrivateKey()).Returns(_ecdsa);
        _keyProvider.Setup(k => k.GetJsonWebKey()).Returns(new JsonWebKey { Kid = _kid });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { })
            .Build();

        _sut = new InternalTokenService(_keyProvider.Object, configuration);
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Sin tenant
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithoutTenant_ContainsSubClaim()
    {
        var userId = Guid.NewGuid();
        var data = new SessionTokenData(userId, null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("sub").Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void SessionToken_WithoutTenant_DoesNotContainTidClaim()
    {
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("tid", out _).Should().BeFalse();
    }

    [Fact]
    public void SessionToken_WithoutTenant_DoesNotContainPermissionClaims()
    {
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("owner", out _).Should().BeFalse();
        jwt.TryGetClaim("groups", out _).Should().BeFalse();
        jwt.TryGetClaim("add", out _).Should().BeFalse();
        jwt.TryGetClaim("exc", out _).Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Owner
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithOwner_ContainsOwnerClaim()
    {
        var tenantId = Guid.NewGuid();
        var data = new SessionTokenData(Guid.NewGuid(), tenantId, true, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("owner").Value.Should().Be("true");
    }

    [Fact]
    public void SessionToken_WithOwner_ContainsTidClaim()
    {
        var tenantId = Guid.NewGuid();
        var data = new SessionTokenData(Guid.NewGuid(), tenantId, true, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("tid").Value.Should().Be(tenantId.ToString());
    }

    [Fact]
    public void SessionToken_WithOwner_DoesNotContainPermissionArrays()
    {
        var data = new SessionTokenData(Guid.NewGuid(), Guid.NewGuid(), true, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("groups", out _).Should().BeFalse();
        jwt.TryGetClaim("add", out _).Should().BeFalse();
        jwt.TryGetClaim("exc", out _).Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Normal (con permisos)
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithPermissions_ContainsGroupsClaim()
    {
        var groups = new[] { "menu:read", "menu:write" };
        var data = new SessionTokenData(Guid.NewGuid(), Guid.NewGuid(), false, groups, [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var groupsClaim = jwt.GetClaim("groups");
        groupsClaim.Should().NotBeNull();
    }

    [Fact]
    public void SessionToken_WithPermissions_ContainsAdditionalScopesClaim()
    {
        var add = new[] { "reservation-service:CancelReservation" };
        var data = new SessionTokenData(Guid.NewGuid(), Guid.NewGuid(), false, [], add, []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var addClaim = jwt.GetClaim("add");
        addClaim.Should().NotBeNull();
    }

    [Fact]
    public void SessionToken_WithPermissions_ContainsExcludedScopesClaim()
    {
        var exc = new[] { "menu-service:SetMenuDepositPolicy" };
        var data = new SessionTokenData(Guid.NewGuid(), Guid.NewGuid(), false, [], [], exc);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var excClaim = jwt.GetClaim("exc");
        excClaim.Should().NotBeNull();
    }

    [Fact]
    public void SessionToken_WithPermissions_DoesNotContainOwnerClaim()
    {
        var data = new SessionTokenData(Guid.NewGuid(), Guid.NewGuid(), false, ["menu:read"], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("owner", out _).Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Firma y expiración
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_IsSignedWithES256()
    {
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void SessionToken_HasCorrectKid()
    {
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().Be(_kid);
    }

    [Fact]
    public async Task SessionToken_CanBeValidated()
    {
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new ECDsaSecurityKey(_ecdsa)
        });

        result.IsValid.Should().BeTrue();
    }
}
