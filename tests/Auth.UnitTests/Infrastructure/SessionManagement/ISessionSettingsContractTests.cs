namespace Auth.UnitTests.Infrastructure.SessionManagement;

public abstract class ISessionSettingsContractTests
{
    protected abstract ISessionSettings CreateSettings();

    [Fact]
    public void Get_ReturnsCookieName()
    {
        var settings = CreateSettings();

        var result = settings.Get();

        result.CookieName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Get_ReturnsPositiveExpirationDays()
    {
        var settings = CreateSettings();

        var result = settings.Get();

        result.ExpirationDays.Should().BeGreaterThan(0);
    }
}

public class SessionSettingsProvider_ContractTests : ISessionSettingsContractTests
{
    protected override ISessionSettings CreateSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Session:CookieName"] = "fudie_session",
                ["Session:ExpirationDays"] = "30"
            })
            .Build();

        return new SessionSettingsProvider(configuration);
    }
}
