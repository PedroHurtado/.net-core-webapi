namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuDeactivateTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly MenuAgg.Deactivate _deactivate;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuDeactivateTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        _addItemToCategory = new(addItem, _createPriceOption, _menuValidator);
        _deactivate = new(_menuValidator);
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

    private MenuAgg CreateMenuWithCategoryAndItem()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));
        return menu;
    }

    [Fact]
    public void Execute_WhenActive_DeactivatesMenu()
    {
        var menu = CreateMenuWithCategoryAndItem();
        menu.IsActive.Should().BeTrue(); // Menu.Create sets IsActive = true by default

        var result = _deactivate.Execute(menu);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithMenuWithoutItems_DeactivatesMenu()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        // Category is empty, but menu is active

        var result = _deactivate.Execute(menu);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ThrowsConflictException()
    {
        var menu = CreateMenuWithCategoryAndItem();
        _deactivate.Execute(menu); // First deactivation

        var act = () => _deactivate.Execute(menu); // Second deactivation

        act.Should().Throw<ConflictException>()
            .WithMessage(DeactivateValidationMessages.MenuAlreadyInactive);
    }

    [Fact]
    public void Execute_PreservesMenuData()
    {
        var menu = CreateMenuWithCategoryAndItem();
        var originalName = menu.Name;
        var originalCategoriesCount = menu.Categories.Count;

        var result = _deactivate.Execute(menu);

        result.Name.Should().Be(originalName);
        result.Categories.Should().HaveCount(originalCategoriesCount);
    }

    [Fact]
    public void Execute_AllowsReactivation()
    {
        var menu = CreateMenuWithCategoryAndItem();
        var activate = new MenuAgg.Activate(_menuValidator);

        _deactivate.Execute(menu);
        menu.IsActive.Should().BeFalse();

        activate.Execute(menu);
        menu.IsActive.Should().BeTrue();
    }
}
