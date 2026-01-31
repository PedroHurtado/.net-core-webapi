namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuUpdateCategoryItemTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly MenuAgg.UpdateCategoryItem _updateCategoryItem;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuUpdateCategoryItemTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        _addItemToCategory = new(addItem, _menuValidator);
        var updateItem = new MenuCategoryEntity.UpdateItem(createCategoryItem, _categoryValidator);
        _updateCategoryItem = new(updateItem, _menuValidator);
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
    public void Execute_WithValidCommand_UpdatesDisplayOrder()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out var menuItem);
        var command = new UpdateCategoryItemCommand(categoryId, menuItem.Id, DisplayOrder: 10, PriceOverrides: null);

        var result = _updateCategoryItem.Execute(menu, command);

        result.Categories.First().Items.First().DisplayOrder.Should().Be(10);
    }

    [Fact]
    public void Execute_WithPriceOverrides_UpdatesPriceOverrides()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out var menuItem);
        var priceOverrides = new HashSet<PriceOption>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 25.00m))
        };
        var command = new UpdateCategoryItemCommand(categoryId, menuItem.Id, DisplayOrder: 0, PriceOverrides: priceOverrides);

        var result = _updateCategoryItem.Execute(menu, command);

        var item = result.Categories.First().Items.First();
        item.PriceOverrides.Should().HaveCount(1);
        item.PriceOverrides.First().Price.Should().Be(25.00m);
    }

    [Fact]
    public void Execute_WithNullPriceOverrides_ClearsPriceOverrides()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out var menuItem);
        var priceOverrides = new HashSet<PriceOption>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 25.00m))
        };
        _updateCategoryItem.Execute(menu, new UpdateCategoryItemCommand(categoryId, menuItem.Id, 0, priceOverrides));

        var result = _updateCategoryItem.Execute(menu, new UpdateCategoryItemCommand(categoryId, menuItem.Id, 0, null));

        result.Categories.First().Items.First().PriceOverrides.Should().BeEmpty();
    }

    [Fact]
    public void Execute_PreservesMenuItem()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out var menuItem);
        var command = new UpdateCategoryItemCommand(categoryId, menuItem.Id, DisplayOrder: 5, PriceOverrides: null);

        var result = _updateCategoryItem.Execute(menu, command);

        var item = result.Categories.First().Items.First();
        item.MenuItem.Id.Should().Be(menuItem.Id);
        item.MenuItem.Name.Should().Be("Caesar Salad");
    }

    [Fact]
    public void Execute_WithNonExistentCategory_ThrowsNotFoundException()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        var command = new UpdateCategoryItemCommand(Guid.NewGuid(), Guid.NewGuid(), DisplayOrder: 0, PriceOverrides: null);

        var act = () => _updateCategoryItem.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage(UpdateCategoryItemValidationMessages.CategoryNotFound);
    }

    [Fact]
    public void Execute_WithNonExistentItem_ThrowsNotFoundException()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out _);
        var command = new UpdateCategoryItemCommand(categoryId, Guid.NewGuid(), DisplayOrder: 0, PriceOverrides: null);

        var act = () => _updateCategoryItem.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage(UpdateItemValidationMessages.ItemNotFoundInCategory);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menu = CreateMenuWithCategoryAndItem(out var categoryId, out var menuItem);
        var command = new UpdateCategoryItemCommand(categoryId, menuItem.Id, DisplayOrder: -1, PriceOverrides: null);

        var act = () => _updateCategoryItem.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
