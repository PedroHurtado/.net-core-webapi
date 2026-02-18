namespace Auth.UnitTests.Infrastructure.SessionManagement;

public class SessionSettingsProviderTests
{
    [Fact]
    public void Get_WithValidConfig_ReturnsSessionSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Session:CookieName"] = "fudie_session",
                ["Session:ExpirationDays"] = "30"
            })
            .Build();

        var settings = new SessionSettingsProvider(configuration);

        var result = settings.Get();

        result.CookieName.Should().Be("fudie_session");
        result.ExpirationDays.Should().Be(30);
    }

    [Fact]
    public void Get_WithMissingCookieName_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Session:ExpirationDays"] = "30"
            })
            .Build();

        var settings = new SessionSettingsProvider(configuration);

        var act = () => settings.Get();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Session:CookieName not found*");
    }

    [Fact]
    public void Get_WithMissingExpirationDays_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Session:CookieName"] = "fudie_session"
            })
            .Build();

        var settings = new SessionSettingsProvider(configuration);

        var act = () => settings.Get();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Session:ExpirationDays not found*");
    }
}
