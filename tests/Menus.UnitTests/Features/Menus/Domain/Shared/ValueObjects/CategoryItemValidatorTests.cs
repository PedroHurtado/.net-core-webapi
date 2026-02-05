namespace Menus.UnitTests.Features.Menus.Domain.Shared.ValueObjects;

public class CategoryItemValidatorTests
{
    private readonly CategoryItemValidator _validator = new();

    private static TestableMenuItem CreateMenuItem() =>
        new(Guid.NewGuid()) { Name = "Test Item", TenantId = Guid.NewGuid() };

    [Fact]
    public void Validate_WithValidCategoryItem_ReturnsSuccess()
    {
        var categoryItem = new TestableCategoryItem(CreateMenuItem(), 0);

        var result = _validator.Validate(categoryItem);

        result.IsValid.Should().BeTrue();
    }

    #region MenuItem Validation

    [Fact]
    public void MenuItem_WhenNull_ReturnsError()
    {
        var categoryItem = new TestableCategoryItem(null!, 0);

        var result = _validator.Validate(categoryItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Menu Item is required");
    }

    #endregion

    #region DisplayOrder Validation

    [Fact]
    public void DisplayOrder_WhenNegative_ReturnsError()
    {
        var categoryItem = new TestableCategoryItem(CreateMenuItem(), -1);

        var result = _validator.Validate(categoryItem);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display Order must be greater than or equal to 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void DisplayOrder_WhenZeroOrPositive_ReturnsSuccess(int displayOrder)
    {
        var categoryItem = new TestableCategoryItem(CreateMenuItem(), displayOrder);

        var result = _validator.Validate(categoryItem);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
