namespace Auth.UnitTests.Infrastructure.Jwt;

public class DevJwtKeyProviderTests
{
    private static IConfiguration BuildConfig(string? privateKey = null, string? kid = null)
    {
        var entries = new Dictionary<string, string?>();

        if (privateKey is not null)
            entries["Jwt:PrivateKey"] = privateKey;

        if (kid is not null)
            entries["Jwt:Kid"] = kid;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(entries)
            .Build();
    }

    private static string GenerateBase64Key()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return Convert.ToBase64String(ecdsa.ExportECPrivateKey());
    }

    [Fact]
    public void GetJsonWebKey_UsesConfiguredKid()
    {
        var provider = new DevJwtKeyProvider(BuildConfig(GenerateBase64Key(), "my-custom-kid"));

        var jwk = provider.GetJsonWebKey();

        jwk.Kid.Should().Be("my-custom-kid");
    }

    [Fact]
    public void GetPrivateKey_WithMissingPrivateKey_ThrowsInvalidOperationException()
    {
        var provider = new DevJwtKeyProvider(BuildConfig(kid: "test-kid"));

        var act = () => provider.GetPrivateKey();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:PrivateKey*");
    }

    [Fact]
    public void Constructor_WithMissingKid_ThrowsInvalidOperationException()
    {
        var act = () => new DevJwtKeyProvider(BuildConfig(GenerateBase64Key()));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Kid*");
    }
}
