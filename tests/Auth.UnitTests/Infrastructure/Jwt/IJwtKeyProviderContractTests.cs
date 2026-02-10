namespace Auth.UnitTests.Infrastructure.Jwt;

public abstract class IJwtKeyProviderContractTests
{
    protected abstract IJwtKeyProvider CreateProvider();

    [Fact]
    public void GetPrivateKey_ReturnsP256Key()
    {
        var provider = CreateProvider();

        var key = provider.GetPrivateKey();

        key.Should().NotBeNull();
        key.KeySize.Should().Be(256);
    }

    [Fact]
    public void GetPrivateKey_ReturnsSameInstance()
    {
        var provider = CreateProvider();

        var key1 = provider.GetPrivateKey();
        var key2 = provider.GetPrivateKey();

        key1.Should().BeSameAs(key2);
    }

    [Fact]
    public void GetJsonWebKey_HasRequiredFields()
    {
        var provider = CreateProvider();

        var jwk = provider.GetJsonWebKey();

        jwk.Kty.Should().Be(JsonWebAlgorithmsKeyTypes.EllipticCurve);
        jwk.Crv.Should().Be("P-256");
        jwk.X.Should().NotBeNullOrEmpty();
        jwk.Y.Should().NotBeNullOrEmpty();
        jwk.Kid.Should().NotBeNullOrEmpty();
        jwk.Use.Should().Be("sig");
        jwk.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void GetJsonWebKey_DoesNotExposePrivateKey()
    {
        var provider = CreateProvider();

        var jwk = provider.GetJsonWebKey();

        jwk.D.Should().BeNullOrEmpty();
    }

    [Fact]
    public void GetJsonWebKey_PublicKeyMatchesPrivateKey()
    {
        var provider = CreateProvider();

        var privateKey = provider.GetPrivateKey();
        var jwk = provider.GetJsonWebKey();

        var exportedParams = privateKey.ExportParameters(includePrivateParameters: false);
        var expectedX = Base64UrlEncoder.Encode(exportedParams.Q.X!);
        var expectedY = Base64UrlEncoder.Encode(exportedParams.Q.Y!);

        jwk.X.Should().Be(expectedX);
        jwk.Y.Should().Be(expectedY);
    }
}

public class DevJwtKeyProvider_ContractTests : IJwtKeyProviderContractTests
{
    protected override IJwtKeyProvider CreateProvider()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var base64Key = Convert.ToBase64String(ecdsa.ExportECPrivateKey());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:PrivateKey"] = base64Key,
                ["Jwt:Kid"] = "contract-test-kid"
            })
            .Build();

        return new DevJwtKeyProvider(configuration);
    }
}
