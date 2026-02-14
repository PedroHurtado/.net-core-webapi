namespace Subscriptions.UnitTests.DomainTests.SharedTests.ValueObjectsTests;

public class SubscriptionFeatureValidatorTests
{
    private readonly SubscriptionFeatureValidator _validator = new();

    [Fact]
    public void Validate_WithValidSubscriptionFeature_ReturnsSuccess()
    {
        var vo = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #region Code Validation

    [Fact]
    public void Code_WhenEmpty_ReturnsError()
    {
        var vo = new SubscriptionFeature("", FeatureType.Boolean, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.CodeRequired);
    }

    [Fact]
    public void Code_WhenExceedsMaxLength_ReturnsError()
    {
        var vo = new SubscriptionFeature(new string('A', 51), FeatureType.Boolean, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.CodeMaxLength);
    }

    [Fact]
    public void Code_WhenLowercase_ReturnsError()
    {
        var vo = new SubscriptionFeature("reservations_monthly", FeatureType.Boolean, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.CodeUppercase);
    }

    [Fact]
    public void Code_WhenContainsSpaces_ReturnsError()
    {
        var vo = new SubscriptionFeature("RESERVATIONS MONTHLY", FeatureType.Boolean, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.CodeNoSpaces);
    }

    #endregion

    #region Limit Validation

    [Fact]
    public void Limit_WhenTypeLimitAndNull_ReturnsError()
    {
        var vo = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.LimitRequiredWhenTypeIsLimit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Limit_WhenNotGreaterThanZero_ReturnsError(int limit)
    {
        var vo = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, limit);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.LimitMustBeGreaterThanZero);
    }

    [Fact]
    public void Limit_WhenTypeBooleanAndHasValue_ReturnsError()
    {
        var vo = new SubscriptionFeature("PRIORITY_SUPPORT", FeatureType.Boolean, 100);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.LimitNotAllowedForBoolean);
    }

    [Fact]
    public void Limit_WhenTypeUnlimitedAndHasValue_ReturnsError()
    {
        var vo = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Unlimited, 100);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == SubscriptionFeatureValidationMessages.LimitNotAllowedForUnlimited);
    }

    #endregion

    #region Valid Combinations

    [Fact]
    public void Validate_WithBooleanTypeAndNullLimit_ReturnsSuccess()
    {
        var vo = new SubscriptionFeature("PRIORITY_SUPPORT", FeatureType.Boolean, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithUnlimitedTypeAndNullLimit_ReturnsSuccess()
    {
        var vo = new SubscriptionFeature("RESERVATIONS_MONTHLY", FeatureType.Unlimited, null);

        var result = _validator.Validate(vo);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
