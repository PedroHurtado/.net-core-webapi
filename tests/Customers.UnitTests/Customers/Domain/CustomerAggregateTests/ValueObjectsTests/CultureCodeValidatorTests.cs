namespace Customers.UnitTests.Customers.Domain.CustomerAggregateTests.ValueObjectsTests;

public class CultureCodeValidatorTests
{
    private readonly CultureCodeValidator _validator = new();

    [Fact]
    public void Validate_WithValidCultureCode_ReturnsSuccess()
    {
        var cultureCode = new CultureCode("es-ES");

        var result = _validator.Validate(cultureCode);

        result.IsValid.Should().BeTrue();
    }

    #region Code Validation

    [Fact]
    public void Code_WhenEmpty_ReturnsError()
    {
        var cultureCode = new CultureCode("");

        var result = _validator.Validate(cultureCode);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CultureCodeValidationMessages.CodeRequired);
    }

    [Theory]
    [InlineData("español")]
    [InlineData("es")]
    public void Code_WhenInvalidFormat_ReturnsError(string code)
    {
        var cultureCode = new CultureCode(code);

        var result = _validator.Validate(cultureCode);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == CultureCodeValidationMessages.CodeFormat);
    }

    [Theory]
    [InlineData("es-ES")]
    [InlineData("en-GB")]
    public void Code_WhenValid_ReturnsSuccess(string code)
    {
        var cultureCode = new CultureCode(code);

        var result = _validator.Validate(cultureCode);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
