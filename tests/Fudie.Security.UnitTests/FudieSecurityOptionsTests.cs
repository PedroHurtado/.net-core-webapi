namespace Fudie.Security.UnitTests;

public class FudieSecurityOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeFudieSecurity()
    {
        FudieSecurityOptions.SectionName.Should().Be("Fudie:Security");
    }

    [Fact]
    public void DefaultCacheRefreshMinutes_ShouldBe60()
    {
        var options = new FudieSecurityOptions();

        options.CacheRefreshMinutes.Should().Be(60);
    }

    [Fact]
    public void DefaultJwksUrl_ShouldBeEmpty()
    {
        var options = new FudieSecurityOptions();

        options.JwksUrl.Should().BeEmpty();
    }
}
