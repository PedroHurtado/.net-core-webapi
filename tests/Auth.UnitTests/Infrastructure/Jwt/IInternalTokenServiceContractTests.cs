namespace Auth.UnitTests.Infrastructure.Jwt;

public abstract class IInternalTokenServiceContractTests
{
    protected abstract IInternalTokenService CreateService();

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
            .AddInMemoryCollection(new Dictionary<string, string?> { })
            .Build();

        return new InternalTokenService(keyProvider.Object, configuration);
    }
}
