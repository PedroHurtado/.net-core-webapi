namespace Auth.UnitTests.Infrastructure.Jwt;

public abstract class IInternalTokenServiceContractTests
{
    protected abstract IInternalTokenService CreateService();

    [Fact]
    public void GenerateTokenInternal_ReturnsNonEmptyString()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateTokenInternal_TokenContainsTidClaim()
    {
        var service = CreateService();
        var tenantId = Guid.NewGuid();

        var token = service.GenerateTokenInternal(tenantId);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.GetClaim("tid").Value.Should().Be(tenantId.ToString());
    }

    [Fact]
    public void GenerateTokenInternal_TokenIsSignedWithES256()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void GenerateTokenInternal_TokenHasExpiration()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateTokenInternal_TokenHasKid()
    {
        var service = CreateService();

        var token = service.GenerateTokenInternal(Guid.NewGuid());

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);
        jwt.Kid.Should().NotBeNullOrEmpty();
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
        return new InternalTokenService(keyProvider.Object);
    }
}
