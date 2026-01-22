namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class FeatureValidatorTests
{
    private readonly FeatureValidator _validator = new();

    [Fact]
    public void Validate_WithValidFeature_ReturnsSuccess()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeTrue();
    }

    #region Code Validation

    [Fact]
    public void Code_WhenEmpty_ReturnsError()
    {
        var feature = new TestableFeature("", "Test Feature", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.CodeRequired);
    }

    [Fact]
    public void Code_WhenExceedsMaxLength_ReturnsError()
    {
        var longCode = new string('A', 51);
        var feature = new TestableFeature(longCode, "Test Feature", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.CodeMaxLength);
    }

    [Theory]
    [InlineData("test_code")]
    [InlineData("Test_Code")]
    [InlineData("TeSt_CoDe")]
    public void Code_WhenNotUppercase_ReturnsError(string code)
    {
        var feature = new TestableFeature(code, "Test Feature", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.CodeMustBeUppercase);
    }

    [Fact]
    public void Code_WhenContainsSpaces_ReturnsError()
    {
        var feature = new TestableFeature("TEST CODE", "Test Feature", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.CodeCannotContainSpaces);
    }

    [Theory]
    [InlineData("TEST_CODE")]
    [InlineData("RESERVATIONS_MONTHLY")]
    [InlineData("ACTIVE_WAITERS")]
    public void Code_WhenValid_ReturnsSuccess(string code)
    {
        var feature = new TestableFeature(code, "Test Feature", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation

    [Fact]
    public void Name_WhenEmpty_ReturnsError()
    {
        var feature = new TestableFeature("TEST_CODE", "", null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var longName = new string('a', 101);
        var feature = new TestableFeature("TEST_CODE", longName, null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.NameMaxLength);
    }

    [Theory]
    [InlineData("Test Feature")]
    [InlineData("Reservas mensuales")]
    [InlineData("Soporte prioritario")]
    public void Name_WhenValid_ReturnsSuccess(string name)
    {
        var feature = new TestableFeature("TEST_CODE", name, null, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        var longDescription = new string('a', 251);
        var feature = new TestableFeature("TEST_CODE", "Test Feature", longDescription, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.DescriptionMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Valid description")]
    public void Description_WhenValid_ReturnsSuccess(string? description)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", description, FeatureType.Boolean);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Limit Validation

    [Fact]
    public void Limit_WhenNullForLimitType_ReturnsError()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, null);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.LimitRequiredForLimitType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Limit_WhenZeroOrNegative_ReturnsError(int limit)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, limit);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.LimitMustBeGreaterThanZero);
    }

    [Fact]
    public void Limit_WhenNotNullForBooleanType_ReturnsError()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Boolean, 100);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.LimitNotAllowedForBooleanType);
    }

    [Fact]
    public void Limit_WhenNotNullForUnlimitedType_ReturnsError()
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Unlimited, 100);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.LimitNotAllowedForUnlimitedType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Limit_WhenValidForLimitType_ReturnsSuccess(int limit)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, limit, "units");

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Unit Validation

    [Fact]
    public void Unit_WhenExceedsMaxLength_ReturnsError()
    {
        var longUnit = new string('a', 51);
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, 100, longUnit);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == FeatureValidationMessages.UnitMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("reservas")]
    [InlineData("camareros")]
    public void Unit_WhenValid_ReturnsSuccess(string? unit)
    {
        var feature = new TestableFeature("TEST_CODE", "Test Feature", null, FeatureType.Limit, 100, unit);

        var result = _validator.Validate(feature);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
