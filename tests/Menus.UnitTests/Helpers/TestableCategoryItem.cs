namespace Menus.UnitTests.Helpers;

public record TestableCategoryItem : CategoryItem
{
    public TestableCategoryItem(MenuItem menuItem, int displayOrder = 0, HashSet<PriceOptionVO>? priceOverrides = null)
        : base(menuItem, displayOrder, priceOverrides) { }
}