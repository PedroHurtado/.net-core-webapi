namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleTokenRequestTests
{
    [Fact]
    public void Record_ShouldHaveExpectedProperties()
    {
        var request = new GoogleTokenRequest(
            Code: "auth-code",
            ClientId: "client-id",
            ClientSecret: "client-secret",
            RedirectUri: "http://localhost/callback");

        request.Code.Should().Be("auth-code");
        request.ClientId.Should().Be("client-id");
        request.ClientSecret.Should().Be("client-secret");
        request.RedirectUri.Should().Be("http://localhost/callback");
        request.GrantType.Should().Be("authorization_code");
    }
}
