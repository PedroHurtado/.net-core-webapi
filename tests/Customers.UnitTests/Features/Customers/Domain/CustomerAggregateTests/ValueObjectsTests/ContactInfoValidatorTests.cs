namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class ContactInfoValidatorTests
{
    private readonly ContactInfoValidator _validator = new();

    [Fact]
    public void Validate_WithValidContactInfo_ReturnsSuccess()
    {
        var contactInfo = new ContactInfo("639079481", "juanjo@example.com", "https://facebook.com/elbardeljuanjo");

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidContactInfo_PhoneOnly_ReturnsSuccess()
    {
        var contactInfo = new ContactInfo("639079481", null, null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeTrue();
    }

    #region Phone Validation

    [Fact]
    public void Phone_WhenEmpty_ReturnsError()
    {
        var contactInfo = new ContactInfo("", null, null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ContactInfoValidationMessages.PhoneRequired);
    }

    [Fact]
    public void Phone_WhenExceedsMaxLength_ReturnsError()
    {
        var contactInfo = new ContactInfo(new string('1', 21), null, null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ContactInfoValidationMessages.PhoneMaxLength);
    }

    #endregion

    #region Email Validation

    [Fact]
    public void Email_WhenExceedsMaxLength_ReturnsError()
    {
        var contactInfo = new ContactInfo("639079481", new string('a', 142) + "@test.com", null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ContactInfoValidationMessages.EmailMaxLength);
    }

    [Fact]
    public void Email_WhenInvalidFormat_ReturnsError()
    {
        var contactInfo = new ContactInfo("639079481", "not-an-email", null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ContactInfoValidationMessages.EmailFormat);
    }

    [Fact]
    public void Email_WhenNull_ReturnsSuccess()
    {
        var contactInfo = new ContactInfo("639079481", null, null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region WebsiteUrl Validation

    [Fact]
    public void WebsiteUrl_WhenExceedsMaxLength_ReturnsError()
    {
        var contactInfo = new ContactInfo("639079481", null, "https://" + new string('a', 493));

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ContactInfoValidationMessages.WebsiteUrlMaxLength);
    }

    [Fact]
    public void WebsiteUrl_WhenInvalidFormat_ReturnsError()
    {
        var contactInfo = new ContactInfo("639079481", null, "not-a-url");

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == ContactInfoValidationMessages.WebsiteUrlFormat);
    }

    [Fact]
    public void WebsiteUrl_WhenNull_ReturnsSuccess()
    {
        var contactInfo = new ContactInfo("639079481", null, null);

        var result = _validator.Validate(contactInfo);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
