namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuAddCategoryTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;

    public MenuAddCategoryTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
    }

    private MenuAgg CreateValidMenu() =>
        _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));

    [Fact]
    public void Execute_WithValidCommand_AddsCategory()
    {
        var menu = CreateValidMenu();
        var command = new AddCategoryCommand("Appetizers");

        var result = _addCategory.Execute(menu, command);

        result.Categories.Should().HaveCount(1);
        result.Categories.First().Name.Should().Be("Appetizers");
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsCategoryFields()
    {
        var menu = CreateValidMenu();
        var command = new AddCategoryCommand(
            Name: "Main Courses",
            Description: "Our signature dishes",
            DisplayOrder: 2);

        var result = _addCategory.Execute(menu, command);

        var category = result.Categories.First();
        category.Name.Should().Be("Main Courses");
        category.Description.Should().Be("Our signature dishes");
        category.DisplayOrder.Should().Be(2);
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithMultipleCategories_AddsAllCategories()
    {
        var menu = CreateValidMenu();

        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        _addCategory.Execute(menu, new AddCategoryCommand("Main Courses"));
        var result = _addCategory.Execute(menu, new AddCategoryCommand("Desserts"));

        result.Categories.Should().HaveCount(3);
    }

    [Fact]
    public void Execute_WithDuplicateName_ThrowsConflictException()
    {
        var menu = CreateValidMenu();
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));

        var act = () => _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));

        act.Should().Throw<ConflictException>()
            .WithMessage(AddCategoryValidationMessages.CategoryNameAlreadyExists);
    }

    [Theory]
    [InlineData("appetizers")]
    [InlineData("APPETIZERS")]
    [InlineData("Appetizers")]
    public void Execute_WithDuplicateNameDifferentCase_ThrowsConflictException(string duplicateName)
    {
        var menu = CreateValidMenu();
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));

        var act = () => _addCategory.Execute(menu, new AddCategoryCommand(duplicateName));

        act.Should().Throw<ConflictException>();
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyCategoryName_ThrowsValidationException(string? name)
    {
        var menu = CreateValidMenu();
        var command = new AddCategoryCommand(name!);

        var act = () => _addCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithCategoryNameExceedingMaxLength_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new AddCategoryCommand(new string('a', 101));

        var act = () => _addCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithCategoryDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new AddCategoryCommand("Appetizers", Description: new string('a', 501));

        var act = () => _addCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new AddCategoryCommand("Appetizers", DisplayOrder: -1);

        var act = () => _addCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
