namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class SocialLinkValidatorTests
{
    private readonly SocialLinkValidator _validator = new();

    [Fact]
    public void Validate_WithValidSocialLink_ReturnsSuccess()
    {
        var socialLink = new SocialLink("Facebook", "https://facebook.com/elbardeljuanjo");

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeTrue();
    }

    #region Platform Validation

    [Fact]
    public void Platform_WhenEmpty_ReturnsError()
    {
        var socialLink = new SocialLink("", "https://facebook.com/elbardeljuanjo");

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SocialLinkValidationMessages.PlatformRequired);
    }

    [Fact]
    public void Platform_WhenExceedsMaxLength_ReturnsError()
    {
        var socialLink = new SocialLink(new string('a', 51), "https://facebook.com/elbardeljuanjo");

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SocialLinkValidationMessages.PlatformMaxLength);
    }

    #endregion

    #region Url Validation

    [Fact]
    public void Url_WhenEmpty_ReturnsError()
    {
        var socialLink = new SocialLink("Facebook", "");

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SocialLinkValidationMessages.UrlRequired);
    }

    [Fact]
    public void Url_WhenExceedsMaxLength_ReturnsError()
    {
        var socialLink = new SocialLink("Facebook", "https://" + new string('a', 493));

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SocialLinkValidationMessages.UrlMaxLength);
    }

    [Fact]
    public void Url_WhenInvalidFormat_ReturnsError()
    {
        var socialLink = new SocialLink("Facebook", "not-a-url");

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SocialLinkValidationMessages.UrlFormat);
    }

    [Theory]
    [InlineData("https://facebook.com/elbardeljuanjo")]
    [InlineData("https://instagram.com/elbardeljuanjo")]
    public void Url_WhenValid_ReturnsSuccess(string url)
    {
        var socialLink = new SocialLink("Facebook", url);

        var result = _validator.Validate(socialLink);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
