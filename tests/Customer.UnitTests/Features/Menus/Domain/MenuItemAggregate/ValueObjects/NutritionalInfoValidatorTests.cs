namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

public class NutritionalInfoValidatorTests
{
    private readonly NutritionalInfoValidator _validator = new();

    [Fact]
    public void Validate_WithValidNutritionalInfo_ReturnsSuccess()
    {
        var info = new TestableNutritionalInfo(
            calories: 500,
            protein: 25.0m,
            carbohydrates: 60.0m,
            fat: 15.0m,
            servingSize: 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeTrue();
    }

    #region Calories Validation

    [Fact]
    public void Calories_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(-1, 25.0m, 60.0m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.CaloriesMin);
    }

    [Fact]
    public void Calories_WhenExceedsMax_ReturnsError()
    {
        var info = new TestableNutritionalInfo(10001, 25.0m, 60.0m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.CaloriesMax);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(500)]
    [InlineData(10000)]
    public void Calories_WhenInRange_ReturnsSuccess(int calories)
    {
        var info = new TestableNutritionalInfo(calories, 25.0m, 60.0m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.Errors.Should().NotContain(e => e.ErrorMessage == NutritionalInfoValidationMessages.CaloriesMin);
        result.Errors.Should().NotContain(e => e.ErrorMessage == NutritionalInfoValidationMessages.CaloriesMax);
    }

    #endregion

    #region Protein Validation

    [Fact]
    public void Protein_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, -1m, 60.0m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ProteinMin);
    }

    [Fact]
    public void Protein_WhenExceedsMax_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 1001m, 60.0m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ProteinMax);
    }

    #endregion

    #region Carbohydrates Validation

    [Fact]
    public void Carbohydrates_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, -1m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.CarbohydratesMin);
    }

    [Fact]
    public void Carbohydrates_WhenExceedsMax_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 1001m, 15.0m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.CarbohydratesMax);
    }

    #endregion

    #region Fat Validation

    [Fact]
    public void Fat_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, -1m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.FatMin);
    }

    [Fact]
    public void Fat_WhenExceedsMax_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 1001m, 350);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.FatMax);
    }

    #endregion

    #region ServingSize Validation

    [Fact]
    public void ServingSize_WhenZero_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 0);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ServingSizeMin);
    }

    [Fact]
    public void ServingSize_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, -1);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ServingSizeMin);
    }

    [Fact]
    public void ServingSize_WhenExceedsMax_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 10001);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ServingSizeMax);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(350)]
    [InlineData(10000)]
    public void ServingSize_WhenInRange_ReturnsSuccess(int servingSize)
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, servingSize);

        var result = _validator.Validate(info);

        result.Errors.Should().NotContain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ServingSizeMin);
        result.Errors.Should().NotContain(e => e.ErrorMessage == NutritionalInfoValidationMessages.ServingSizeMax);
    }

    #endregion

    #region Optional Fields Validation

    [Fact]
    public void Fiber_WhenNullOrValid_ReturnsSuccess()
    {
        var infoNull = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 350, fiber: null);
        var infoValid = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 350, fiber: 5.0m);

        _validator.Validate(infoNull).IsValid.Should().BeTrue();
        _validator.Validate(infoValid).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Fiber_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 350, fiber: -1m);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.FiberMin);
    }

    [Fact]
    public void Sugar_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 350, sugar: -1m);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.SugarMin);
    }

    [Fact]
    public void Salt_WhenNegative_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 350, salt: -1m);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.SaltMin);
    }

    [Fact]
    public void Salt_WhenExceedsMax_ReturnsError()
    {
        var info = new TestableNutritionalInfo(500, 25.0m, 60.0m, 15.0m, 350, salt: 101m);

        var result = _validator.Validate(info);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == NutritionalInfoValidationMessages.SaltMax);
    }

    #endregion
}
