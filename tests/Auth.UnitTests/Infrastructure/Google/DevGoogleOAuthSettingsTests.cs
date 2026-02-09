namespace Auth.UnitTests.Infrastructure.Google;

public class DevGoogleOAuthSettingsTests
{
    [Fact]
    public void Get_WithValidConfig_ReturnsGoogleOAuthSettings()
    {
        var json = """
            {"web":{"client_id":"test-client-id","project_id":"test-project","auth_uri":"https://accounts.google.com/o/oauth2/auth","token_uri":"https://oauth2.googleapis.com/token","auth_provider_x509_cert_url":"https://www.googleapis.com/oauth2/v1/certs","client_secret":"test-client-secret","redirect_uris":["http://localhost:5176/login/google"]}}
            """;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Google:Oauth"] = json
            })
            .Build();

        var settings = new DevGoogleOAuthSettings(configuration);

        var result = settings.Get();

        result.ClientId.Should().Be("test-client-id");
        result.ClientSecret.Should().Be("test-client-secret");
        result.RedirectUri.Should().Be("http://localhost:5176/login/google");
        result.AuthUri.Should().Be("https://accounts.google.com/o/oauth2/auth");
        result.TokenUri.Should().Be("https://oauth2.googleapis.com/token");
        result.CertsUri.Should().Be("https://www.googleapis.com/oauth2/v1/certs");
    }

    [Fact]
    public void Get_WithMissingConfig_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder().Build();
        var settings = new DevGoogleOAuthSettings(configuration);

        var act = () => settings.Get();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Google:Oauth secret not found*");
    }
}
