namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuRemoveCategoryTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _categoryItemValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.RemoveCategory _removeCategory;
    private readonly MenuCategoryEntity.AddItem _addItem;
    private readonly MenuItemAgg.Create _createMenuItem;
    private readonly PriceOption.Create _createPriceOption;

    public MenuRemoveCategoryTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _removeCategory = new(_menuValidator);

        var createCategoryItem = new CategoryItemVO.Create(_categoryItemValidator);
        _addItem = new(createCategoryItem, _categoryValidator);

        _createPriceOption = new(_priceOptionValidator);
        _createMenuItem = new(_createPriceOption, _menuItemValidator);
    }

    private MenuAgg CreateMenuWithCategory(out Guid categoryId)
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Menu"
        ));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers", "Starter dishes", 1));
        categoryId = menu.Categories.First().Id;
        return menu;
    }

    private MenuItem CreateMenuItem()
    {
        CreatePriceOptionCommand[] priceOptions = [new(PortionType.Full, 10.99m)];
        return _createMenuItem.Execute(new CreateMenuItemCommand(
            Guid.NewGuid(),
            "Test Item",
            null,
            null,
            0,
            false,
            false,
            null,
            true,
            [],
            null,
            priceOptions));
    }

    [Fact]
    public void Execute_WithEmptyCategory_RemovesCategory()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var command = new RemoveCategoryCommand(categoryId);

        var result = _removeCategory.Execute(menu, command);

        result.Categories.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithMultipleCategories_RemovesOnlySpecifiedCategory()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        _addCategory.Execute(menu, new AddCategoryCommand("Main Courses"));
        _addCategory.Execute(menu, new AddCategoryCommand("Desserts"));
        var command = new RemoveCategoryCommand(categoryId);

        var result = _removeCategory.Execute(menu, command);

        result.Categories.Should().HaveCount(2);
        result.Categories.Should().NotContain(c => c.Id == categoryId);
        result.Categories.Select(c => c.Name).Should().BeEquivalentTo(["Main Courses", "Desserts"]);
    }

    [Fact]
    public void Execute_WithNonExistentCategory_ThrowsNotFoundException()
    {
        var menu = CreateMenuWithCategory(out _);
        var nonExistentId = Guid.NewGuid();
        var command = new RemoveCategoryCommand(nonExistentId);

        var act = () => _removeCategory.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Execute_WithCategoryContainingItems_ThrowsValidationException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var category = menu.Categories.First();
        var menuItem = CreateMenuItem();
        _addItem.Execute(category, new AddItemCommand(menuItem));
        var command = new RemoveCategoryCommand(categoryId);

        var act = () => _removeCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{RemoveCategoryValidationMessages.CannotRemoveCategoryWithItems}*");
    }
}