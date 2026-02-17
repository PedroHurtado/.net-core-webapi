namespace Auth.UnitTests.Infrastructure.Customers;

public class InternalAuthHandlerTests
{
    private readonly Guid _platformTenantId = Guid.NewGuid();
    private readonly Mock<IInternalTokenService> _tokenService = new();
    private readonly Mock<IConfiguration> _configuration = new();

    public InternalAuthHandlerTests()
    {
        _configuration
            .Setup(c => c["Fudie:PlatformTenantId"])
            .Returns(_platformTenantId.ToString());
    }

    [Fact]
    public async Task SendAsync_AddsAuthorizationBearerHeader()
    {
        var expectedToken = "test-jwt-token";
        _tokenService
            .Setup(t => t.GenerateTokenInternal(_platformTenantId))
            .Returns(expectedToken);

        var handler = new InternalAuthHandler(_tokenService.Object, _configuration.Object)
        {
            InnerHandler = new FakeInnerHandler()
        };
        var client = new HttpClient(handler);

        await client.GetAsync("http://localhost/customer");

        FakeInnerHandler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        FakeInnerHandler.LastRequest.Headers.Authorization.Parameter.Should().Be(expectedToken);
    }

    [Fact]
    public async Task SendAsync_GeneratesTokenWithPlatformTenantId()
    {
        _tokenService
            .Setup(t => t.GenerateTokenInternal(_platformTenantId))
            .Returns("any-token");

        var handler = new InternalAuthHandler(_tokenService.Object, _configuration.Object)
        {
            InnerHandler = new FakeInnerHandler()
        };
        var client = new HttpClient(handler);

        await client.GetAsync("http://localhost/customer");

        _tokenService.Verify(t => t.GenerateTokenInternal(_platformTenantId), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ForwardsRequestToInnerHandler()
    {
        _tokenService
            .Setup(t => t.GenerateTokenInternal(It.IsAny<Guid>()))
            .Returns("any-token");

        var handler = new InternalAuthHandler(_tokenService.Object, _configuration.Object)
        {
            InnerHandler = new FakeInnerHandler()
        };
        var client = new HttpClient(handler);

        var response = await client.GetAsync("http://localhost/customer");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private class FakeInnerHandler : DelegatingHandler
    {
        public static HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
