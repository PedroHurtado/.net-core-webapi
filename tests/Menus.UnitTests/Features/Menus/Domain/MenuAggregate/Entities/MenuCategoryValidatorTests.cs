namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Entities;

public class MenuCategoryValidatorTests
{
    private readonly MenuCategoryValidator _validator = new();

    [Fact]
    public void Validate_WithValidCategory_ReturnsSuccess()
    {
        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            Description = "Starters and snacks",
            DisplayOrder = 1,
            IsActive = true
        };

        var result = _validator.Validate(category);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithParameterlessConstructor_ReturnsErrors()
    {
        var category = new TestableMenuCategory();

        var result = _validator.Validate(category);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.IdRequired);
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.NameRequired);
    }

    #region Id Validation

    [Fact]
    public void Id_WhenEmpty_ReturnsError()
    {
        var category = new TestableMenuCategory(Guid.Empty) { Name = "Valid Name" };

        var result = _validator.Validate(category);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.IdRequired);
    }

    #endregion

    #region Name Validation

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Name_WhenEmpty_ReturnsError(string? name)
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { Name = name! };

        var result = _validator.Validate(category);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.NameRequired);
    }

    [Fact]
    public void Name_WhenExceedsMaxLength_ReturnsError()
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { Name = new string('a', 101) };

        var result = _validator.Validate(category);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.NameMaxLength);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Appetizers")]
    public void Name_WhenValidLength_ReturnsSuccess(string name)
    {
        var category = new TestableMenuCategory(Guid.NewGuid()) { Name = name };

        var result = _validator.Validate(category);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuCategoryValidationMessages.NameMaxLength);
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Description_WhenExceedsMaxLength_ReturnsError()
    {
        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            Description = new string('a', 501)
        };

        var result = _validator.Validate(category);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.DescriptionMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Short description")]
    public void Description_WhenValidOrEmpty_ReturnsSuccess(string? description)
    {
        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            Description = description
        };

        var result = _validator.Validate(category);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuCategoryValidationMessages.DescriptionMaxLength);
    }

    #endregion

    #region DisplayOrder Validation

    [Fact]
    public void DisplayOrder_WhenNegative_ReturnsError()
    {
        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            DisplayOrder = -1
        };

        var result = _validator.Validate(category);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == MenuCategoryValidationMessages.DisplayOrderMin);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void DisplayOrder_WhenZeroOrPositive_ReturnsSuccess(int displayOrder)
    {
        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            DisplayOrder = displayOrder
        };

        var result = _validator.Validate(category);

        result.Errors.Should().NotContain(e => e.ErrorMessage == MenuCategoryValidationMessages.DisplayOrderMin);
    }

    #endregion
}
