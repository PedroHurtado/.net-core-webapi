namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class SocialLinkTests
{
    #region Platform

    [Theory]
    [InlineData("Facebook")]
    [InlineData("Instagram")]
    [InlineData("TripAdvisor")]
    public void Platform_SetsCorrectValue(string platform)
    {
        var socialLink = new SocialLink(platform, "https://example.com");

        socialLink.Platform.Should().Be(platform);
    }

    #endregion

    #region Url

    [Theory]
    [InlineData("https://facebook.com/elbardeljuanjo")]
    [InlineData("https://instagram.com/elbardeljuanjo")]
    public void Url_SetsCorrectValue(string url)
    {
        var socialLink = new SocialLink("Facebook", url);

        socialLink.Url.Should().Be(url);
    }

    #endregion
}
