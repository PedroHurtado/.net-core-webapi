namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.MenuCategory;

public class MenuCategoryRemoveItemTests
{
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuCategoryEntity.Create _createCategory;
    private readonly MenuCategoryEntity.AddItem _addItem;
    private readonly MenuCategoryEntity.RemoveItem _removeItem;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuCategoryRemoveItemTests()
    {
        _createCategory = new(_categoryValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        _addItem = new(createCategoryItem, _categoryValidator);
        _removeItem = new(_categoryValidator);
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

    private MenuCategoryEntity CreateCategoryWithItem(out MenuItem menuItem)
    {
        var category = _createCategory.Execute(new CreateCategoryCommand("Appetizers"));
        menuItem = CreateMenuItem();
        _addItem.Execute(category, new AddItemCommand(menuItem));
        return category;
    }

    [Fact]
    public void Execute_WithValidCommand_RemovesItem()
    {
        var category = CreateCategoryWithItem(out var menuItem);

        var result = _removeItem.Execute(category, new RemoveItemCommand(menuItem.Id));

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithMultipleItems_RemovesOnlySpecifiedItem()
    {
        var category = _createCategory.Execute(new CreateCategoryCommand("Appetizers"));
        var menuItem1 = CreateMenuItem("Caesar Salad");
        var menuItem2 = CreateMenuItem("Bruschetta");
        _addItem.Execute(category, new AddItemCommand(menuItem1));
        _addItem.Execute(category, new AddItemCommand(menuItem2));

        var result = _removeItem.Execute(category, new RemoveItemCommand(menuItem1.Id));

        result.Items.Should().HaveCount(1);
        result.Items.First().MenuItem.Id.Should().Be(menuItem2.Id);
    }

    [Fact]
    public void Execute_WithNonExistentItem_ThrowsNotFoundException()
    {
        var category = _createCategory.Execute(new CreateCategoryCommand("Appetizers"));
        var command = new RemoveItemCommand(Guid.NewGuid());

        var act = () => _removeItem.Execute(category, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage(RemoveItemValidationMessages.ItemNotFoundInCategory);
    }

    [Fact]
    public void Execute_RemoveAllItems_LeavesEmptyCategory()
    {
        var category = _createCategory.Execute(new CreateCategoryCommand("Appetizers"));
        var menuItem1 = CreateMenuItem("Caesar Salad");
        var menuItem2 = CreateMenuItem("Bruschetta");
        _addItem.Execute(category, new AddItemCommand(menuItem1));
        _addItem.Execute(category, new AddItemCommand(menuItem2));

        _removeItem.Execute(category, new RemoveItemCommand(menuItem1.Id));
        var result = _removeItem.Execute(category, new RemoveItemCommand(menuItem2.Id));

        result.Items.Should().BeEmpty();
    }
}
