namespace Auth.UnitTests.Features.Sessions.Api.Queries;

public class GetJwksTests
{
    private readonly Mock<IJwtKeyProvider> _jwtKeyProvider = new();
    private readonly GetJwks.Service _service;

    public GetJwksTests()
    {
        _service = new GetJwks.Service(_jwtKeyProvider.Object);
    }

    [Fact]
    public void Handler_ReturnsOkWithResponse()
    {
        var expectedResponse = new GetJwks.Response([
            new GetJwks.JwkEntry("EC", "P-256", "x", "y", "kid", "sig", "ES256")
        ]);

        var mockService = new Mock<GetJwks.IService>();
        mockService.Setup(s => s.Handle()).Returns(expectedResponse);

        var result = GetJwks.Handler(mockService.Object);

        var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<GetJwks.Response>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public void Handle_ReturnsJwksWithOneKey()
    {
        var jwk = new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
            Crv = "P-256",
            X = "test-x-value",
            Y = "test-y-value",
            Kid = "test-kid-001",
            Use = "sig",
            Alg = SecurityAlgorithms.EcdsaSha256
        };

        _jwtKeyProvider.Setup(p => p.GetJsonWebKey()).Returns(jwk);

        var response = _service.Handle();

        response.Keys.Should().HaveCount(1);
        var key = response.Keys.First();
        key.Kty.Should().Be("EC");
        key.Crv.Should().Be("P-256");
        key.X.Should().Be("test-x-value");
        key.Y.Should().Be("test-y-value");
        key.Kid.Should().Be("test-kid-001");
        key.Use.Should().Be("sig");
        key.Alg.Should().Be("ES256");
    }
}
