namespace Auth.UnitTests.Infrastructure.Customers;

public class InternalAuthHandlerTests
{
    private readonly string _internalSecret = Guid.NewGuid().ToString();
    private readonly Mock<IConfiguration> _configuration = new();

    public InternalAuthHandlerTests()
    {
        _configuration
            .Setup(c => c["Fudie:InternalSecret"])
            .Returns(_internalSecret);
    }

    [Fact]
    public async Task SendAsync_AddsInternalKeyHeader()
    {
        var handler = new InternalAuthHandler(_configuration.Object)
        {
            InnerHandler = new FakeInnerHandler()
        };
        var client = new HttpClient(handler);

        await client.GetAsync("http://localhost/customer");

        FakeInnerHandler.LastRequest!.Headers
            .GetValues("X-Internal-Key").First()
            .Should().Be(_internalSecret);
    }

    [Fact]
    public async Task SendAsync_ForwardsRequestToInnerHandler()
    {
        var handler = new InternalAuthHandler(_configuration.Object)
        {
            InnerHandler = new FakeInnerHandler()
        };
        var client = new HttpClient(handler);

        var response = await client.GetAsync("http://localhost/customer");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenInternalSecretNotConfigured()
    {
        var emptyConfig = new Mock<IConfiguration>();

        var handler = new InternalAuthHandler(emptyConfig.Object)
        {
            InnerHandler = new FakeInnerHandler()
        };
        var client = new HttpClient(handler);

        var act = () => client.GetAsync("http://localhost/customer");

        await act.Should().ThrowAsync<InvalidOperationException>();
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
