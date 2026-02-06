namespace Customers.UnitTests.Features.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class BillingInfoValidatorTests
{
    private readonly BillingInfoValidator _validator = new();

    private static readonly Address ValidAddress = new(
        "Ctra. Murcia, 23",
        "La Puebla de Mula",
        "30193",
        "Murcia",
        "España",
        new GeoPoint(38.0389m, -1.4917m));

    [Fact]
    public void Validate_WithValidBillingInfo_ReturnsSuccess()
    {
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", ValidAddress);

        var result = _validator.Validate(billingInfo);

        result.IsValid.Should().BeTrue();
    }

    #region BusinessName Validation

    [Fact]
    public void BusinessName_WhenEmpty_ReturnsError()
    {
        var billingInfo = new BillingInfo("", "B12345678", ValidAddress);

        var result = _validator.Validate(billingInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == BillingInfoValidationMessages.BusinessNameRequired);
    }

    [Fact]
    public void BusinessName_WhenExceedsMaxLength_ReturnsError()
    {
        var billingInfo = new BillingInfo(new string('a', 201), "B12345678", ValidAddress);

        var result = _validator.Validate(billingInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == BillingInfoValidationMessages.BusinessNameMaxLength);
    }

    #endregion

    #region TaxId Validation

    [Fact]
    public void TaxId_WhenEmpty_ReturnsError()
    {
        var billingInfo = new BillingInfo("Bar Juanjo SL", "", ValidAddress);

        var result = _validator.Validate(billingInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == BillingInfoValidationMessages.TaxIdRequired);
    }

    [Fact]
    public void TaxId_WhenExceedsMaxLength_ReturnsError()
    {
        var billingInfo = new BillingInfo("Bar Juanjo SL", new string('a', 51), ValidAddress);

        var result = _validator.Validate(billingInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == BillingInfoValidationMessages.TaxIdMaxLength);
    }

    #endregion

    #region BillingAddress Validation

    [Fact]
    public void BillingAddress_WhenNull_ReturnsError()
    {
        var billingInfo = new BillingInfo("Bar Juanjo SL", "B12345678", null!);

        var result = _validator.Validate(billingInfo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == BillingInfoValidationMessages.BillingAddressRequired);
    }

    #endregion
}
