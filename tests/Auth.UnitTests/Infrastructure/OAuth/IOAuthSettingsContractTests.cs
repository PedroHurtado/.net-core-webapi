namespace Auth.UnitTests.Infrastructure.OAuth;

public abstract class IOAuthSettingsContractTests
{
    protected abstract IOAuthSettings CreateSettings();

    [Fact]
    public void Get_ReturnsStateCookieName()
    {
        var settings = CreateSettings();

        var result = settings.Get();

        result.StateCookieName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Get_ReturnsPositiveStateExpirationSeconds()
    {
        var settings = CreateSettings();

        var result = settings.Get();

        result.StateExpirationSeconds.Should().BeGreaterThan(0);
    }
}

public class OAuthSettingsProvider_ContractTests : IOAuthSettingsContractTests
{
    protected override IOAuthSettings CreateSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OAuth:StateCookieName"] = "fudie_oauth_state",
                ["OAuth:StateExpirationSeconds"] = "300"
            })
            .Build();

        return new OAuthSettingsProvider(configuration);
    }
}
