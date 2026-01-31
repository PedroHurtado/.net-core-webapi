namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuActivateTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly MenuAgg.Activate _activate;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuActivateTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        _addItemToCategory = new(addItem, _menuValidator);
        _activate = new(_menuValidator);
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
    public void Execute_WithCategoriesAndItems_ActivatesMenu()
    {
        var menu = CreateMenuWithCategoryAndItem();
        menu.IsActive.Should().BeTrue(); // Menu.Create sets IsActive = true by default

        // First deactivate to test activation
        var deactivate = new MenuAgg.Deactivate(_menuValidator);
        deactivate.Execute(menu);

        var result = _activate.Execute(menu);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithMultipleCategoriesOneWithItems_ActivatesMenu()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        _addCategory.Execute(menu, new AddCategoryCommand("Desserts")); // Empty category
        var categoryId = menu.Categories.First().Id;
        var menuItem = CreateMenuItem();
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));

        // Deactivate first
        var deactivate = new MenuAgg.Deactivate(_menuValidator);
        deactivate.Execute(menu);

        var result = _activate.Execute(menu);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
    {
        var menu = CreateMenuWithCategoryAndItem();
        // Menu is already active by default

        var act = () => _activate.Execute(menu);

        act.Should().Throw<ConflictException>()
            .WithMessage(ActivateValidationMessages.MenuAlreadyActive);
    }

    [Fact]
    public void Execute_WithNoCategories_ThrowsValidationException()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));

        // Deactivate first
        var deactivate = new MenuAgg.Deactivate(_menuValidator);
        deactivate.Execute(menu);

        var act = () => _activate.Execute(menu);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ActivateValidationMessages.MenuMustHaveAtLeastOneCategory}*");
    }

    [Fact]
    public void Execute_WithEmptyCategories_ThrowsValidationException()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        _addCategory.Execute(menu, new AddCategoryCommand("Desserts"));
        // Both categories are empty

        // Deactivate first
        var deactivate = new MenuAgg.Deactivate(_menuValidator);
        deactivate.Execute(menu);

        var act = () => _activate.Execute(menu);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ActivateValidationMessages.MenuMustHaveAtLeastOneCategoryWithItems}*");
    }
}
