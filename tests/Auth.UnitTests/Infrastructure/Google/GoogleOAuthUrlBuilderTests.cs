namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleOAuthUrlBuilderTests
{
    private static GoogleOAuthUrlBuilder CreateBuilder()
    {
        var settings = new GoogleOAuthSettings(
            ClientId: "test-client-id",
            ClientSecret: "test-secret",
            RedirectUri: "http://localhost:5176/auth/login/google",
            AuthUri: "https://accounts.google.com/o/oauth2/auth",
            TokenUri: "https://oauth2.googleapis.com/token",
            CertsUri: "https://www.googleapis.com/oauth2/v1/certs");

        var mockSettings = new Mock<IGoogleOAuthSettings>();
        mockSettings.Setup(s => s.Get()).Returns(settings);

        return new GoogleOAuthUrlBuilder(mockSettings.Object);
    }

    [Fact]
    public void Build_WithValidSettings_ReturnsUrlWithExpectedParams()
    {
        var builder = CreateBuilder();

        var result = builder.Build();

        result.Url.Should().Contain("client_id=test-client-id");
        result.Url.Should().Contain("redirect_uri=");
        result.Url.Should().Contain("response_type=code");
        result.Url.Should().Contain("scope=openid");
        result.Url.Should().Contain("access_type=online");
        result.Url.Should().Contain("state=");
        result.State.Should().NotBeEmpty();
    }

    [Fact]
    public void Build_CalledTwice_GeneratesUniqueState()
    {
        var builder = CreateBuilder();

        var first = builder.Build();
        var second = builder.Build();

        first.State.Should().NotBe(second.State);
    }
}
