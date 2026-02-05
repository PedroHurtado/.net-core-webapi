namespace Menus.UnitTests.Features.Menus.Domain.Shared.Commands.CategoryItem;

public class CategoryItemCreateTests
{
    private readonly CategoryItemValidator _validator = new();
    private readonly PriceOptionValidator _priceOptionValidator = new();
    private readonly MenuItemValidator _menuItemValidator = new();
    private readonly CategoryItemVO.Create _create;
    private readonly PriceOptionVO.Create _createPriceOption;
    private readonly MenuItemAgg.Create _createMenuItem;

    public CategoryItemCreateTests()
    {
        _create = new(_validator);
        _createPriceOption = new(_priceOptionValidator);
        _createMenuItem = new(_createPriceOption, _menuItemValidator);
    }

    private MenuItem CreateMenuItem() =>
        _createMenuItem.Execute(new CreateMenuItemCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Item",
            Description: null,
            ImageUrl: null,
            DisplayOrder: 0,
            IsHighRiskItem: false,
            RequiresAdvanceOrder: false,
            MinimumAdvanceOrderQuantity: null,
            IsAlwaysAvailable: true,
            AvailableDays: [],
            AllergenNotes: null,
            PriceOptions: [new CreatePriceOptionCommand(PortionType.Full, 12.99m)]));

    [Fact]
    public void Execute_WithValidCommand_ReturnsCategoryItem()
    {
        var menuItem = CreateMenuItem();
        var command = new CreateCategoryItemCommand(menuItem);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.MenuItem.Should().Be(menuItem);
        result.DisplayOrder.Should().Be(0);
        result.PriceOverrides.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Execute_WithDisplayOrder_SetsDisplayOrder(int displayOrder)
    {
        var command = new CreateCategoryItemCommand(CreateMenuItem(), displayOrder);

        var result = _create.Execute(command);

        result.DisplayOrder.Should().Be(displayOrder);
    }

    [Fact]
    public void Execute_WithPriceOverrides_SetsPriceOverrides()
    {
        var priceOverrides = new HashSet<PriceOptionVO>
        {
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.00m)),
            _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Half, 8.00m))
        };
        var command = new CreateCategoryItemCommand(CreateMenuItem(), 0, priceOverrides);

        var result = _create.Execute(command);

        result.PriceOverrides.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithNullPriceOverrides_ReturnsEmptyCollection()
    {
        var command = new CreateCategoryItemCommand(CreateMenuItem(), 0, null);

        var result = _create.Execute(command);

        result.PriceOverrides.Should().BeEmpty();
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithNullMenuItem_ThrowsValidationException()
    {
        var command = new CreateCategoryItemCommand(null!);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var command = new CreateCategoryItemCommand(CreateMenuItem(), -1);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
