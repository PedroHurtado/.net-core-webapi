namespace Customer.UnitTests.Features.Menus.Domain.Shared.Commands.CategoryItem;

public class CategoryItemCreateTests
{
    private readonly CategoryItemValidator _validator = new();
    private readonly CategoryItemVO.Create _create;

    public CategoryItemCreateTests()
    {
        _create = new(_validator);
    }

    private static TestableMenuItem CreateMenuItem() =>
        new(Guid.NewGuid()) { Name = "Test Item", TenantId = Guid.NewGuid() };

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
            new TestablePriceOption(PortionType.Full, 15.00m),
            new TestablePriceOption(PortionType.Half, 8.00m)
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
