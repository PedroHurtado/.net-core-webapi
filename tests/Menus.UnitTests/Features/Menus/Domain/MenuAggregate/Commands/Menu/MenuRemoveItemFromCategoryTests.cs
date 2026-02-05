namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuRemoveItemFromCategoryTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly MenuAgg.RemoveItemFromCategory _removeItemFromCategory;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuRemoveItemFromCategoryTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        _addItemToCategory = new(addItem, _createPriceOption, _menuValidator);
        var removeItem = new MenuCategoryEntity.RemoveItem(_categoryValidator);
        _removeItemFromCategory = new(removeItem, _menuValidator);
        _createMenuItem = new(_createPriceOption, _menuItemValidator);
    }

    private MenuItem CreateMenuItem(string name = "Caesar Salad")
    {
        return _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: name,
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 12.99m)]
        ));
    }

    private MenuAgg CreateMenuWithCategoryAndItem(out Guid categoryId, out MenuItem menuItem)
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        categoryId = menu.Categories.First().Id;
        menuItem = CreateMenuItem();
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));
        return menu;
    }

    [Fact]
    public void Execute_WithValidCommand_RemovesItemFromCategory()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out var menuItem);
        var command = new RemoveItemFromCategoryCommand(categoryId, menuItem.Id);

        var result = _removeItemFromCategory.Execute(menu, command);

        result.Categories.First().Items.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithMultipleItems_RemovesOnlySpecifiedItem()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        var categoryId = menu.Categories.First().Id;
        var menuItem1 = CreateMenuItem("Caesar Salad");
        var menuItem2 = CreateMenuItem("Bruschetta");
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem1));
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem2));

        var result = _removeItemFromCategory.Execute(menu, new RemoveItemFromCategoryCommand(categoryId, menuItem1.Id));

        result.Categories.First().Items.Should().HaveCount(1);
        result.Categories.First().Items.First().MenuItem.Id.Should().Be(menuItem2.Id);
    }

    [Fact]
    public void Execute_WithNonExistentCategory_ThrowsKeyNotFoundException()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        var command = new RemoveItemFromCategoryCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = () => _removeItemFromCategory.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Execute_WithNonExistentItem_ThrowsKeyNotFoundException()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out _);
        var command = new RemoveItemFromCategoryCommand(categoryId, Guid.NewGuid());

        var act = () => _removeItemFromCategory.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Execute_RemoveAllItems_LeavesEmptyCategory()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        var categoryId = menu.Categories.First().Id;
        var menuItem1 = CreateMenuItem("Caesar Salad");
        var menuItem2 = CreateMenuItem("Bruschetta");
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem1));
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem2));

        _removeItemFromCategory.Execute(menu, new RemoveItemFromCategoryCommand(categoryId, menuItem1.Id));
        var result = _removeItemFromCategory.Execute(menu, new RemoveItemFromCategoryCommand(categoryId, menuItem2.Id));

        result.Categories.First().Items.Should().BeEmpty();
    }
}
