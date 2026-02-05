namespace Menus.UnitTests.Features.Menus.Domain.Shared.ValueObjects;

public class CategoryItemTests
{
    private static TestableMenuItem CreateMenuItem(Guid? id = null) =>
        new(id ?? Guid.NewGuid()) { Name = "Test Item", TenantId = Guid.NewGuid() };

    [Fact]
    public void MenuItem_SetsCorrectValue()
    {
        var menuItem = CreateMenuItem();

        var categoryItem = new TestableCategoryItem(menuItem);

        categoryItem.MenuItem.Should().Be(menuItem);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void DisplayOrder_SetsCorrectValue(int displayOrder)
    {
        var menuItem = CreateMenuItem();

        var categoryItem = new TestableCategoryItem(menuItem, displayOrder);

        categoryItem.DisplayOrder.Should().Be(displayOrder);
    }

    [Fact]
    public void PriceOverrides_WhenNull_ReturnsEmptyCollection()
    {
        var menuItem = CreateMenuItem();

        var categoryItem = new TestableCategoryItem(menuItem, 0, null);

        categoryItem.PriceOverrides.Should().BeEmpty();
    }

    [Fact]
    public void PriceOverrides_WhenProvided_ReturnsCorrectCollection()
    {
        var menuItem = CreateMenuItem();
        var priceOverrides = new HashSet<PriceOptionVO>
        {
            new TestablePriceOption(PortionType.Full, 10.00m),
            new TestablePriceOption(PortionType.Half, 5.00m)
        };

        var categoryItem = new TestableCategoryItem(menuItem, 0, priceOverrides);

        categoryItem.PriceOverrides.Should().HaveCount(2);
    }

    #region Equality

    [Fact]
    public void Equals_WithSameMenuItem_ReturnsTrue()
    {
        var menuItemId = Guid.NewGuid();
        var menuItem1 = CreateMenuItem(menuItemId);
        var menuItem2 = CreateMenuItem(menuItemId);

        var categoryItem1 = new TestableCategoryItem(menuItem1, 0);
        var categoryItem2 = new TestableCategoryItem(menuItem2, 1);

        categoryItem1.Equals(categoryItem2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentMenuItem_ReturnsFalse()
    {
        var menuItem1 = CreateMenuItem();
        var menuItem2 = CreateMenuItem();

        var categoryItem1 = new TestableCategoryItem(menuItem1);
        var categoryItem2 = new TestableCategoryItem(menuItem2);

        categoryItem1.Equals(categoryItem2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var categoryItem = new TestableCategoryItem(CreateMenuItem());

        categoryItem.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameMenuItemId_ReturnsSameHashCode()
    {
        var menuItemId = Guid.NewGuid();
        var categoryItem1 = new TestableCategoryItem(CreateMenuItem(menuItemId));
        var categoryItem2 = new TestableCategoryItem(CreateMenuItem(menuItemId));

        categoryItem1.GetHashCode().Should().Be(categoryItem2.GetHashCode());
    }

    #endregion
}
