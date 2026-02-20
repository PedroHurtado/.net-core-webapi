namespace Auth.UnitTests.Features.Users.Api.UserAggregate.Commands;

public class InitiateGoogleLoginTests
{
    [Fact]
    public void Handle_ReturnsUrlFromBuilder()
    {
        var expectedUrl = new GoogleOAuthUrl("https://accounts.google.com/auth?client_id=test", "random-state");
        var urlBuilder = new Mock<IGoogleOAuthUrlBuilder>();
        urlBuilder.Setup(b => b.Build()).Returns(expectedUrl);
        var service = new InitiateGoogleLogin.Service(urlBuilder.Object);

        var result = service.Handle();

        result.Should().Be(expectedUrl);
    }

    [Fact]
    public void Handler_SetsOAuthStateCookieAndRedirects()
    {
        var mockService = new Mock<InitiateGoogleLogin.IService>();
        mockService.Setup(s => s.Handle())
            .Returns(new GoogleOAuthUrl("https://accounts.google.com/auth", "test-state"));
        var mockOAuthSettings = new Mock<IOAuthSettings>();
        mockOAuthSettings.Setup(s => s.Get()).Returns(new OAuthSettings("fudie_oauth_state", 300));
        var httpContext = new DefaultHttpContext();

        var result = InitiateGoogleLogin.Handler(mockService.Object, mockOAuthSettings.Object, httpContext);

        result.Should().BeOfType<RedirectHttpResult>()
            .Which.Url.Should().Be("https://accounts.google.com/auth");
        httpContext.Response.Headers["Set-Cookie"].ToString()
            .Should().Contain("fudie_oauth_state=test-state");
    }
}
