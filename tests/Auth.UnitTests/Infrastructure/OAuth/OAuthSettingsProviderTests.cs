namespace Auth.UnitTests.Infrastructure.OAuth;

public class OAuthSettingsProviderTests
{
    [Fact]
    public void Get_WithValidConfig_ReturnsOAuthSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OAuth:StateCookieName"] = "fudie_oauth_state",
                ["OAuth:StateExpirationSeconds"] = "300"
            })
            .Build();

        var settings = new OAuthSettingsProvider(configuration);

        var result = settings.Get();

        result.StateCookieName.Should().Be("fudie_oauth_state");
        result.StateExpirationSeconds.Should().Be(300);
    }

    [Fact]
    public void Get_WithMissingStateCookieName_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OAuth:StateExpirationSeconds"] = "300"
            })
            .Build();

        var settings = new OAuthSettingsProvider(configuration);

        var act = () => settings.Get();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OAuth:StateCookieName not found*");
    }

    [Fact]
    public void Get_WithMissingStateExpirationSeconds_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OAuth:StateCookieName"] = "fudie_oauth_state"
            })
            .Build();

        var settings = new OAuthSettingsProvider(configuration);

        var act = () => settings.Get();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*OAuth:StateExpirationSeconds not found*");
    }
}
