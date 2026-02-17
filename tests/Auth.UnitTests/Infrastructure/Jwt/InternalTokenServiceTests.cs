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
        _sut = new InternalTokenService(_keyProvider.Object);
    }

    [Fact]
    public void GenerateTokenInternal_ReturnsNonEmptyString()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateTokenInternal_TokenContainsTidClaim()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.GetClaim("tid").Value.Should().Be(tenantId.ToString());
    }

    [Fact]
    public void GenerateTokenInternal_TokenIsSignedWithES256()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void GenerateTokenInternal_TokenHasExpiration()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateTokenInternal_TokenHasCorrectKid()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.Kid.Should().Be(_kid);
    }

    [Fact]
    public async Task GenerateTokenInternal_TokenCanBeValidated()
    {
        var tenantId = Guid.NewGuid();

        var token = _sut.GenerateTokenInternal(tenantId);

        var handler = new JsonWebTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new ECDsaSecurityKey(_ecdsa)
        };

        var result = await handler.ValidateTokenAsync(token, validationParams);
        result.IsValid.Should().BeTrue();
    }
}
