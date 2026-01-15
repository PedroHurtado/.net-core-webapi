namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.MenuCategory;

public class MenuCategoryAddItemTests
{
    private readonly MenuCategoryValidator _categoryValidator = new();
    private readonly CategoryItemValidator _itemValidator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuCategoryEntity.AddItem _addItem;

    public MenuCategoryAddItemTests()
    {
        var createCategoryItem = new CategoryItemVO.Create(_itemValidator);
        _addItem = new(createCategoryItem, _categoryValidator);
    }

    private static TestableMenuCategory CreateValidCategory() => new(Guid.NewGuid())
    {
        Name = "Appetizers",
        IsActive = true,
        DisplayOrder = 0
    };

    private static MenuItem CreateMenuItem() => new TestableMenuItem(Guid.NewGuid())
    {
        TenantId = Guid.NewGuid(),
        Name = "Caesar Salad",
        IsActive = true,
        DisplayOrder = 0
    };

    [Fact]
    public void Execute_WithValidCommand_AddsItem()
    {
        var category = CreateValidCategory();
        var menuItem = CreateMenuItem();
        var command = new AddItemCommand(menuItem);

        var result = _addItem.Execute(category, command);

        result.Items.Should().HaveCount(1);
        result.Items.First().MenuItem.Id.Should().Be(menuItem.Id);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsItemFields()
    {
        var category = CreateValidCategory();
        var menuItem = CreateMenuItem();
        var createPriceOption = new PriceOptionVO.Create(_priceOptionValidator);
        var priceOverrides = new HashSet<PriceOption>
        {
            createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.99m))
        };
        var command = new AddItemCommand(
            MenuItem: menuItem,
            DisplayOrder: 5,
            PriceOverrides: priceOverrides);

        var result = _addItem.Execute(category, command);

        var item = result.Items.First();
        item.DisplayOrder.Should().Be(5);
        item.PriceOverrides.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithMultipleItems_AddsAllItems()
    {
        var category = CreateValidCategory();
        var menuItem1 = CreateMenuItem();
        var menuItem2 = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Bruschetta",
            IsActive = true,
            DisplayOrder = 0
        };

        _addItem.Execute(category, new AddItemCommand(menuItem1));
        var result = _addItem.Execute(category, new AddItemCommand(menuItem2));

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithDuplicateItem_ThrowsConflictException()
    {
        var category = CreateValidCategory();
        var menuItem = CreateMenuItem();
        _addItem.Execute(category, new AddItemCommand(menuItem));

        var act = () => _addItem.Execute(category, new AddItemCommand(menuItem));

        act.Should().Throw<ConflictException>()
            .WithMessage(AddItemValidationMessages.ItemAlreadyExists);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var category = CreateValidCategory();
        var menuItem = CreateMenuItem();
        var command = new AddItemCommand(menuItem, DisplayOrder: -1);

        var act = () => _addItem.Execute(category, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
