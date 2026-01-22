namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class PaymentProviderConfigValidatorTests
{
    private readonly PaymentProviderConfigValidator _validator = new();

    [Fact]
    public void Validate_WithValidConfig_ReturnsSuccess()
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #region Provider Validation

    [Fact]
    public void Provider_WhenEmpty_ReturnsError()
    {
        var config = new TestablePaymentProviderConfig("", "prod_123", "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PaymentProviderConfigValidationMessages.ProviderRequired);
    }

    [Fact]
    public void Provider_WhenExceedsMaxLength_ReturnsError()
    {
        var longProvider = new string('a', 51);
        var config = new TestablePaymentProviderConfig(longProvider, "prod_123", "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PaymentProviderConfigValidationMessages.ProviderMaxLength);
    }

    [Theory]
    [InlineData("Stripe")]
    [InlineData("Paddle")]
    [InlineData("PayPal")]
    public void Provider_WhenValid_ReturnsSuccess(string provider)
    {
        var config = new TestablePaymentProviderConfig(provider, "prod_123", "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ExternalProductId Validation

    [Fact]
    public void ExternalProductId_WhenEmpty_ReturnsError()
    {
        var config = new TestablePaymentProviderConfig("Stripe", "", "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PaymentProviderConfigValidationMessages.ExternalProductIdRequired);
    }

    [Fact]
    public void ExternalProductId_WhenExceedsMaxLength_ReturnsError()
    {
        var longProductId = new string('a', 101);
        var config = new TestablePaymentProviderConfig("Stripe", longProductId, "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PaymentProviderConfigValidationMessages.ExternalProductIdMaxLength);
    }

    [Theory]
    [InlineData("prod_123")]
    [InlineData("product_abc")]
    [InlineData("ext_prod_xyz")]
    public void ExternalProductId_WhenValid_ReturnsSuccess(string externalProductId)
    {
        var config = new TestablePaymentProviderConfig("Stripe", externalProductId, "price_456");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ExternalPriceId Validation

    [Fact]
    public void ExternalPriceId_WhenEmpty_ReturnsError()
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", "");

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PaymentProviderConfigValidationMessages.ExternalPriceIdRequired);
    }

    [Fact]
    public void ExternalPriceId_WhenExceedsMaxLength_ReturnsError()
    {
        var longPriceId = new string('a', 101);
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", longPriceId);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == PaymentProviderConfigValidationMessages.ExternalPriceIdMaxLength);
    }

    [Theory]
    [InlineData("price_123")]
    [InlineData("price_abc")]
    [InlineData("ext_price_xyz")]
    public void ExternalPriceId_WhenValid_ReturnsSuccess(string externalPriceId)
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", externalPriceId);

        var result = _validator.Validate(config);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
