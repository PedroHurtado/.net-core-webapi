namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.CommandsTests;

public class SocialLinkCreateTests(DomainFixture fixture) : IClassFixture<DomainFixture>
{
    private readonly SocialLink.Create _create = fixture.Get<SocialLink.Create>();

    [Fact]
    public void Execute_WithValidCommand_ReturnsSocialLink()
    {
        var command = new CreateSocialLinkCommand("Facebook", "https://facebook.com/elbardeljuanjo");

        var result = _create.Execute(command);

        result.Platform.Should().Be("Facebook");
        result.Url.Should().Be("https://facebook.com/elbardeljuanjo");
    }

    [Fact]
    public void Execute_WithInstagram_ReturnsSocialLink()
    {
        var command = new CreateSocialLinkCommand("Instagram", "https://instagram.com/elbardeljuanjo");

        var result = _create.Execute(command);

        result.Platform.Should().Be("Instagram");
        result.Url.Should().Be("https://instagram.com/elbardeljuanjo");
    }

    [Fact]
    public void Execute_WithTripAdvisor_ReturnsSocialLink()
    {
        var command = new CreateSocialLinkCommand("TripAdvisor", "https://tripadvisor.es/Customer_Review-g1087583");

        var result = _create.Execute(command);

        result.Platform.Should().Be("TripAdvisor");
    }

    [Fact]
    public void Execute_WithEmptyPlatform_ThrowsValidationException()
    {
        var command = new CreateSocialLinkCommand("", "https://facebook.com/elbardeljuanjo");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SocialLinkValidationMessages.PlatformRequired}*");
    }

    [Fact]
    public void Execute_WithEmptyUrl_ThrowsValidationException()
    {
        var command = new CreateSocialLinkCommand("Facebook", "");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SocialLinkValidationMessages.UrlRequired}*");
    }

    [Fact]
    public void Execute_WithInvalidUrl_ThrowsValidationException()
    {
        var command = new CreateSocialLinkCommand("Facebook", "not-a-url");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SocialLinkValidationMessages.UrlFormat}*");
    }
}
