namespace Auth.UnitTests.Infrastructure.Jwt;

public abstract class IInternalTokenServiceContractTests
{
    protected abstract IInternalTokenService CreateService();

    // ──────────────────────────────────────────────
    // GenerateTokenInternal(Guid tenantId)
    // ──────────────────────────────────────────────

    [Fact]
    public void WithTenantId_ReturnsNonEmptyString()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WithTenantId_TokenContainsTidClaim()
    {
        var service = CreateService();
        var tenantId = Guid.NewGuid();

        var token = service.GenerateTokenInternal(tenantId);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("tid").Value.Should().Be(tenantId.ToString());
    }

    [Fact]
    public void WithTenantId_TokenContainsIkClaim()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("ik").Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WithTenantId_TokenIsSignedWithES256()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void WithTenantId_TokenHasExpiration()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void WithTenantId_TokenHasKid()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().NotBeNullOrEmpty();
    }

    // ──────────────────────────────────────────────
    // GenerateTokenInternal()
    // ──────────────────────────────────────────────

    [Fact]
    public void WithoutTenantId_ReturnsNonEmptyString()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal();

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WithoutTenantId_TokenContainsIkClaim()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("ik").Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WithoutTenantId_TokenDoesNotContainTidClaim()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("tid", out _).Should().BeFalse();
    }

    [Fact]
    public void WithoutTenantId_TokenIsSignedWithES256()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void WithoutTenantId_TokenHasKid()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal();

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().NotBeNullOrEmpty();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_ReturnsNonEmptyString()
    {
        var service = CreateService();
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = service.GenerateSessionToken(data);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SessionToken_ContainsSubClaim()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();
        var data = new SessionTokenData(userId, null, false, [], [], []);

        var token = service.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("sub").Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void SessionToken_IsSignedWithES256()
    {
        var service = CreateService();
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = service.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void SessionToken_HasKid()
    {
        var service = CreateService();
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = service.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SessionToken_HasExpiration()
    {
        var service = CreateService();
        var data = new SessionTokenData(Guid.NewGuid(), null, false, [], [], []);

        var token = service.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }
}

public class InternalTokenService_ContractTests : IInternalTokenServiceContractTests
{
    private readonly ECDsa _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    protected override IInternalTokenService CreateService()
    {
        var keyProvider = new Mock<IJwtKeyProvider>();
        keyProvider.Setup(k => k.GetPrivateKey()).Returns(_ecdsa);
        keyProvider.Setup(k => k.GetJsonWebKey()).Returns(new JsonWebKey { Kid = "contract-test-kid" });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:InternalSecret"] = Guid.NewGuid().ToString()
            })
            .Build();

        return new InternalTokenService(keyProvider.Object, configuration);
    }
}
