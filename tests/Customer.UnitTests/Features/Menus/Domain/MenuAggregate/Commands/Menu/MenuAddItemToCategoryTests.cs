namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuAddItemToCategoryTests
{
    private readonly MenuValidator _menuValidator = new();
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuAgg.Create _createMenu;
    private readonly MenuAgg.AddCategory _addCategory;
    private readonly MenuAgg.AddItemToCategory _addItemToCategory;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuAddItemToCategoryTests()
    {
        _createMenu = new(_menuValidator);
        var createCategory = new MenuCategoryEntity.Create(_categoryValidator);
        _addCategory = new(createCategory, _menuValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        var addItem = new MenuCategoryEntity.AddItem(createCategoryItem, _categoryValidator);
        _addItemToCategory = new(addItem, _menuValidator);
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

    private MenuAgg CreateMenuWithCategory(out Guid categoryId)
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        _addCategory.Execute(menu, new AddCategoryCommand("Appetizers"));
        categoryId = menu.Categories.First().Id;
        return menu;
    }

    [Fact]
    public void Execute_WithValidCommand_AddsItemToCategory()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var menuItem = CreateMenuItem();
        var command = new AddItemToCategoryCommand(categoryId, menuItem);

        var result = _addItemToCategory.Execute(menu, command);

        result.Categories.First().Items.Should().HaveCount(1);
        result.Categories.First().Items.First().MenuItem.Id.Should().Be(menuItem.Id);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsItemFields()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var menuItem = CreateMenuItem();
        var priceOverrides = new HashSet<PriceOption>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 18.99m))
        };
        var command = new AddItemToCategoryCommand(categoryId, menuItem, DisplayOrder: 5, PriceOverrides: priceOverrides);

        var result = _addItemToCategory.Execute(menu, command);

        var item = result.Categories.First().Items.First();
        item.DisplayOrder.Should().Be(5);
        item.PriceOverrides.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithMultipleItems_AddsAllItems()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var menuItem1 = CreateMenuItem("Caesar Salad");
        var menuItem2 = CreateMenuItem("Bruschetta");

        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem1));
        var result = _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem2));

        result.Categories.First().Items.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithNonExistentCategory_ThrowsNotFoundException()
    {
        var menu = _createMenu.Execute(new CreateMenuCommand(Guid.NewGuid(), "Test Menu"));
        var menuItem = CreateMenuItem();
        var command = new AddItemToCategoryCommand(Guid.NewGuid(), menuItem);

        var act = () => _addItemToCategory.Execute(menu, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage(AddItemToCategoryValidationMessages.CategoryNotFound);
    }

    [Fact]
    public void Execute_WithDuplicateItem_ThrowsConflictException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var menuItem = CreateMenuItem();
        _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));

        var act = () => _addItemToCategory.Execute(menu, new AddItemToCategoryCommand(categoryId, menuItem));

        act.Should().Throw<ConflictException>()
            .WithMessage(AddItemValidationMessages.ItemAlreadyExists);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menu = CreateMenuWithCategory(out var categoryId);
        var menuItem = CreateMenuItem();
        var command = new AddItemToCategoryCommand(categoryId, menuItem, DisplayOrder: -1);

        var act = () => _addItemToCategory.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
