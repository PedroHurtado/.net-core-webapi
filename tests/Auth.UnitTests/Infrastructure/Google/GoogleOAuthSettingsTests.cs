namespace Auth.UnitTests.Infrastructure.Google;

public class GoogleOAuthSettingsTests
{
    [Fact]
    public void Record_ShouldHaveExpectedProperties()
    {
        var settings = new GoogleOAuthSettings(
            ClientId: "client-id",
            ClientSecret: "client-secret",
            RedirectUri: "http://localhost/callback",
            AuthUri: "https://accounts.google.com/o/oauth2/auth",
            TokenUri: "https://oauth2.googleapis.com/token",
            CertsUri: "https://www.googleapis.com/oauth2/v1/certs");

        settings.ClientId.Should().Be("client-id");
        settings.ClientSecret.Should().Be("client-secret");
        settings.RedirectUri.Should().Be("http://localhost/callback");
        settings.AuthUri.Should().Be("https://accounts.google.com/o/oauth2/auth");
        settings.TokenUri.Should().Be("https://oauth2.googleapis.com/token");
        settings.CertsUri.Should().Be("https://www.googleapis.com/oauth2/v1/certs");
    }
}
