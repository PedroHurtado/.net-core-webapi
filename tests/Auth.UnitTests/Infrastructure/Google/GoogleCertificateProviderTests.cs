namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleCertificateProviderTests
{
    private readonly Mock<IGoogleCertsApi> _certsApi = new();

    private GoogleCertificateProvider CreateProvider() => new(_certsApi.Object);

    private static Dictionary<string, string> CreateValidCertificates(int count = 1)
    {
        var certs = new Dictionary<string, string>();
        for (var i = 0; i < count; i++)
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                $"CN=Test{i}",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
            certs[$"key{i}"] = cert.ExportCertificatePem();
        }
        return certs;
    }

    [Fact]
    public async Task GetSigningKeysAsync_WithValidCerts_ReturnsSecurityKeys()
    {
        var certs = CreateValidCertificates(2);
        _certsApi.Setup(c => c.GetCertificatesAsync()).ReturnsAsync(certs);
        var provider = CreateProvider();

        var keys = await provider.GetSigningKeysAsync();

        keys.Should().HaveCount(2);
        keys.Should().AllBeOfType<X509SecurityKey>();
    }

    [Fact]
    public async Task GetSigningKeysAsync_WhenCalledTwice_ReturnsCachedKeys()
    {
        var certs = CreateValidCertificates();
        _certsApi.Setup(c => c.GetCertificatesAsync()).ReturnsAsync(certs);
        var provider = CreateProvider();

        var first = await provider.GetSigningKeysAsync();
        var second = await provider.GetSigningKeysAsync();

        first.Should().BeSameAs(second);
        _certsApi.Verify(c => c.GetCertificatesAsync(), Times.Once());
    }

    [Fact]
    public async Task GetSigningKeysAsync_ConcurrentCalls_FetchesOnlyOnce()
    {
        var certs = CreateValidCertificates();
        _certsApi.Setup(c => c.GetCertificatesAsync()).ReturnsAsync(certs);
        var provider = CreateProvider();

        var tasks = Enumerable.Range(0, 10).Select(_ => provider.GetSigningKeysAsync());
        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(keys => keys.Should().BeSameAs(results[0]));
        _certsApi.Verify(c => c.GetCertificatesAsync(), Times.Once());
    }
}