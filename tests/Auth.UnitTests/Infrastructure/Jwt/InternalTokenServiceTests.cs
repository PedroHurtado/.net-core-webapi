namespace Auth.UnitTests.Infrastructure.Jwt;

public class InternalTokenServiceTests
{
    private readonly ECDsa _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly string _kid = "test-kid";
    private readonly string _internalSecret = Guid.NewGuid().ToString();
    private readonly Mock<IJwtKeyProvider> _keyProvider = new();
    private readonly InternalTokenService _sut;

    public InternalTokenServiceTests()
    {
        _keyProvider.Setup(k => k.GetPrivateKey()).Returns(_ecdsa);
        _keyProvider.Setup(k => k.GetJsonWebKey()).Returns(new JsonWebKey { Kid = _kid });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:InternalSecret"] = _internalSecret
            })
            .Build();

        _sut = new InternalTokenService(_keyProvider.Object, configuration);
    }

    // ──────────────────────────────────────────────
    // GenerateTokenInternal(Guid tenantId)
    // ──────────────────────────────────────────────

    [Fact]
    public void WithTenantId_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateTokenInternal(Guid.NewGuid());

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WithTenantId_TokenContainsTidClaim()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("tid").Value.Should().Be(tenantId.ToString());
    }

    [Fact]
    public void WithTenantId_TokenContainsIkClaim()
    {
        var token = _sut.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("ik").Value.Should().Be(_internalSecret);
    }

    [Fact]
    public void WithTenantId_TokenIsSignedWithES256()
    {
        var token = _sut.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void WithTenantId_TokenHasExpiration()
    {
        var token = _sut.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void WithTenantId_TokenHasCorrectKid()
    {
        var token = _sut.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().Be(_kid);
    }

    [Fact]
    public async Task WithTenantId_TokenCanBeValidated()
    {
        var token = _sut.GenerateTokenInternal(Guid.NewGuid());

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

    // ──────────────────────────────────────────────
    // GenerateTokenInternal()
    // ──────────────────────────────────────────────

    [Fact]
    public void WithoutTenantId_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateTokenInternal();

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WithoutTenantId_TokenContainsIkClaim()
    {
        var token = _sut.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("ik").Value.Should().Be(_internalSecret);
    }

    [Fact]
    public void WithoutTenantId_TokenDoesNotContainTidClaim()
    {
        var token = _sut.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("tid", out _).Should().BeFalse();
    }

    [Fact]
    public void WithoutTenantId_TokenIsSignedWithES256()
    {
        var token = _sut.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void WithoutTenantId_TokenHasCorrectKid()
    {
        var token = _sut.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().Be(_kid);
    }

    [Fact]
    public async Task WithoutTenantId_TokenCanBeValidated()
    {
        var token = _sut.GenerateTokenInternal();

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
