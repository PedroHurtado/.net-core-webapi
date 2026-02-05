namespace Menus.UnitTests.Features.Menus.Domain.AllergenAggregate;

public class AllergenValidatorTests
{
    private readonly AllergenValidator _validator = new();

    [Fact]
    public void Validate_WithValidAllergen_ReturnsSuccess()
    {
        var allergen = new TestableAllergen("GLUTEN")
        {
            Name = "Gluten",
            IconUrl = "https://example.com/icon.png",
            IsActive = true,
            DisplayOrder = 1
        };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithParameterlessConstructor_ReturnsErrors()
    {
        var allergen = new TestableAllergen();

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.CodeRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.NameRequired);
    }

    #region Code Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Code_WhenEmpty_ReturnsError(string? code)
    {
        var allergen = new TestableAllergen(code!) { Name = "Valid Name" };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.CodeRequired);
    }

    [Fact]
    public void Code_WhenExceedsMaxLength_ReturnsError()
    {
        var allergen = new TestableAllergen(new string('A', 21)) { Name = "Valid Name" };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.CodeMaxLength);
    }

    [Theory]
    [InlineData("gluten")]
    [InlineData("Gluten")]
    [InlineData("GLUTEN WHEAT")]
    [InlineData("GLUTEN-WHEAT")]
    public void Code_WhenInvalidFormat_ReturnsError(string code)
    {
        var allergen = new TestableAllergen(code) { Name = "Valid Name" };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.CodeFormat);
    }

    [Theory]
    [InlineData("GLUTEN")]
    [InlineData("LACTEOS")]
    [InlineData("GLUTEN_FREE")]
    [InlineData("A1")]
    public void Code_WhenValidFormat_ReturnsSuccess(string code)
    {
        var allergen = new TestableAllergen(code) { Name = "Valid Name" };

        var result = _validator.Validate(allergen);

        result.Errors.Should().NotContain(e => e.ErrorMessage == AllergenValidationMessages.CodeFormat);
    }

    #endregion

    #region Name Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Name_WhenEmpty_ReturnsError(string? name)
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = name! };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = new string('a', 101) };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.NameMaxLength);
    }

    #endregion

    #region IconUrl Validation

    [Fact]
    public void IconUrl_WhenExceedsMaxLength_ReturnsError()
    {
        var allergen = new TestableAllergen("GLUTEN")
        {
            Name = "Gluten",
            IconUrl = "https://example.com/" + new string('a', 500)
        };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.IconUrlMaxLength);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/icon.png")]
    [InlineData("//example.com/icon.png")]
    public void IconUrl_WhenInvalidUrl_ReturnsError(string iconUrl)
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = "Gluten", IconUrl = iconUrl };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.IconUrlInvalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://example.com/icon.png")]
    [InlineData("http://example.com/icon.png")]
    public void IconUrl_WhenValidOrEmpty_ReturnsSuccess(string? iconUrl)
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = "Gluten", IconUrl = iconUrl };

        var result = _validator.Validate(allergen);

        result.Errors.Should().NotContain(e => e.ErrorMessage == AllergenValidationMessages.IconUrlInvalid);
    }

    #endregion

    #region DisplayOrder Validation

    [Fact]
    public void DisplayOrder_WhenNegative_ReturnsError()
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = "Gluten", DisplayOrder = -1 };

        var result = _validator.Validate(allergen);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == AllergenValidationMessages.DisplayOrderMin);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void DisplayOrder_WhenZeroOrPositive_ReturnsSuccess(int displayOrder)
    {
        var allergen = new TestableAllergen("GLUTEN") { Name = "Gluten", DisplayOrder = displayOrder };

        var result = _validator.Validate(allergen);

        result.Errors.Should().NotContain(e => e.ErrorMessage == AllergenValidationMessages.DisplayOrderMin);
    }

    #endregion
}
