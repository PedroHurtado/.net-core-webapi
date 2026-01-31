namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.MenuCategory;

public class MenuCategoryUpdateItemTests
{
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();

    private readonly MenuCategoryEntity.Create _createCategory;
    private readonly MenuCategoryEntity.AddItem _addItem;
    private readonly MenuCategoryEntity.UpdateItem _updateItem;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public MenuCategoryUpdateItemTests()
    {
        _createCategory = new(_categoryValidator);
        _createPriceOption = new(_priceOptionValidator);
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        _addItem = new(createCategoryItem, _categoryValidator);
        _updateItem = new(createCategoryItem, _categoryValidator);
        _createMenuItem = new(_createPriceOption, _menuItemValidator);
    }

    private MenuCategoryEntity CreateCategoryWithItem(out MenuItem menuItem)
    {
        var category = _createCategory.Execute(new CreateCategoryCommand("Appetizers"));

        menuItem = _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Caesar Salad",
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

        _addItem.Execute(category, new AddItemCommand(menuItem));
        return category;
    }

    [Fact]
    public void Execute_WithValidCommand_UpdatesDisplayOrder()
    {
        var category = CreateCategoryWithItem(out var menuItem);
        var command = new UpdateItemCommand(menuItem.Id, DisplayOrder: 5, PriceOverrides: null);

        var result = _updateItem.Execute(category, command);

        result.Items.First().DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_WithPriceOverrides_UpdatesPriceOverrides()
    {
        var category = CreateCategoryWithItem(out var menuItem);
        var priceOverrides = new HashSet<PriceOption>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 18.99m))
        };
        var command = new UpdateItemCommand(menuItem.Id, DisplayOrder: 0, PriceOverrides: priceOverrides);

        var result = _updateItem.Execute(category, command);

        result.Items.First().PriceOverrides.Should().HaveCount(1);
        result.Items.First().PriceOverrides.First().Price.Should().Be(18.99m);
    }

    [Fact]
    public void Execute_WithNullPriceOverrides_ClearsPriceOverrides()
    {
        var category = CreateCategoryWithItem(out var menuItem);
        var priceOverrides = new HashSet<PriceOption>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 18.99m))
        };
        _updateItem.Execute(category, new UpdateItemCommand(menuItem.Id, 0, priceOverrides));

        var result = _updateItem.Execute(category, new UpdateItemCommand(menuItem.Id, 0, null));

        result.Items.First().PriceOverrides.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithNonExistentItem_ThrowsNotFoundException()
    {
        var category = _createCategory.Execute(new CreateCategoryCommand("Appetizers"));
        var command = new UpdateItemCommand(Guid.NewGuid(), DisplayOrder: 0, PriceOverrides: null);

        var act = () => _updateItem.Execute(category, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage(UpdateItemValidationMessages.ItemNotFoundInCategory);
    }

    [Fact]
    public void Execute_PreservesMenuItem()
    {
        var category = CreateCategoryWithItem(out var menuItem);
        var command = new UpdateItemCommand(menuItem.Id, DisplayOrder: 10, PriceOverrides: null);

        var result = _updateItem.Execute(category, command);

        result.Items.First().MenuItem.Id.Should().Be(menuItem.Id);
        result.Items.First().MenuItem.Name.Should().Be("Caesar Salad");
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var category = CreateCategoryWithItem(out var menuItem);
        var command = new UpdateItemCommand(menuItem.Id, DisplayOrder: -1, PriceOverrides: null);

        var act = () => _updateItem.Execute(category, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
