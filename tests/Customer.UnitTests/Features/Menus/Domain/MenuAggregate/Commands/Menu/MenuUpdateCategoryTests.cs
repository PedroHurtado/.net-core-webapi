namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuUpdateCategoryTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.UpdateCategory _updateCategory;

    public MenuUpdateCategoryTests()
    {
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        var updateCategoryDetails = new MenuCategoryEntity.Update(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _updateCategory = new(updateCategoryDetails, _menuValidator);
    }

    private TestableMenu CreateMenuWithCategory(out Guid categoryId)
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Test Menu",
            IsActive = true,
            DisplayOrder = 0
        };
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers", "Starter dishes", 1));
        categoryId = menu.Categories.First().Id;
        return menu;
    }

    [Fact]
    public void Execute_WithValidCommand_UpdatesCategory()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Starters",
            Description: "Updated description",
            DisplayOrder: 5);

        var result = _updateCategory.Execute(menu, command);

        var category = result.Categories.First();
        category.Name.Should().Be("Starters");
        category.Description.Should().Be("Updated description");
        category.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_WithNullDescription_ClearsDescription()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        menu.Categories.First().Description.Should().NotBeNull();
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Appetizers",
            Description: null,
            DisplayOrder: 0);

        var result = _updateCategory.Execute(menu, command);

        result.Categories.First().Description.Should().BeNull();
    }

    [Fact]
    public void Execute_WithSameName_UpdatesSuccessfully()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Appetizers",
            Description: "New description",
            DisplayOrder: 10);

        var result = _updateCategory.Execute(menu, command);

        var category = result.Categories.First();
        category.Name.Should().Be("Appetizers");
        category.Description.Should().Be("New description");
    }

    [Fact]
    public void Execute_WithNonExistentCategory_ThrowsNotFoundException()
    {
        var menu = CreateMenuWithCategory(out _);
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateCategoryCommand(
            CategoryId: nonExistentId,
            Name: "New Name",
            Description: null,
            DisplayOrder: 0);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Execute_WithDuplicateName_ThrowsConflictException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        _addCategory.Execute(menu, new AddCategoryCommand("Main Courses"));
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Main Courses",
            Description: null,
            DisplayOrder: 0);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<ConflictException>()
            .WithMessage(UpdateCategoryValidationMessages.CategoryNameAlreadyExists);
    }

    [Theory]
    [InlineData("main courses")]
    [InlineData("MAIN COURSES")]
    [InlineData("Main Courses")]
    public void Execute_WithDuplicateNameDifferentCase_ThrowsConflictException(string duplicateName)
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        _addCategory.Execute(menu, new AddCategoryCommand("Main Courses"));
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: duplicateName,
            Description: null,
            DisplayOrder: 0);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<ConflictException>();
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: name!,
            Description: null,
            DisplayOrder: 0);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: new string('a', 101),
            Description: null,
            DisplayOrder: 0);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Appetizers",
            Description: new string('a', 501),
            DisplayOrder: 0);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Appetizers",
            Description: null,
            DisplayOrder: -1);

        var act = () => _updateCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
