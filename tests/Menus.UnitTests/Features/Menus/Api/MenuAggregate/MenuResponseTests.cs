namespace Menus.UnitTests.Features.Menus.Api.MenuAggregate;

public class MenuResponseTests
{
    #region MenuResponse.Map

    [Fact]
    public void MenuResponse_Map_MapsAllProperties()
    {
        var menuId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateTime(2024, 1, 1);
        var effectiveUntil = new DateTime(2024, 12, 31);
        var depositPolicy = new TestableDepositPolicy(DepositType.FixedAmount, 50m);

        var menu = new TestableMenu(menuId)
        {
            TenantId = tenantId,
            Name = "Lunch Menu",
            Description = "Daily specials",
            IsActive = true,
            DisplayOrder = 1,
            EffectiveFrom = effectiveFrom,
            EffectiveUntil = effectiveUntil,
            DepositPolicy = depositPolicy
        };

        var response = MenuResponse.Map(menu);

        response.Id.Should().Be(menuId);
        response.Name.Should().Be("Lunch Menu");
        response.Description.Should().Be("Daily specials");
        response.IsActive.Should().BeTrue();
        response.DisplayOrder.Should().Be(1);
        response.EffectiveFrom.Should().Be(effectiveFrom);
        response.EffectiveUntil.Should().Be(effectiveUntil);
        response.DepositPolicy.Should().NotBeNull();
    }

    [Fact]
    public void MenuResponse_Map_WithNullOptionalFields_MapsNullValues()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Simple Menu"
        };

        var response = MenuResponse.Map(menu);

        response.Description.Should().BeNull();
        response.EffectiveFrom.Should().BeNull();
        response.EffectiveUntil.Should().BeNull();
        response.DepositPolicy.Should().BeNull();
    }

    [Fact]
    public void MenuResponse_Map_WithCategories_MapsCategories()
    {
        var menu = new TestableMenu(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Test Menu"
        };

        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            IsActive = true
        };
        menu.AddCategoryDirect(category);

        var response = MenuResponse.Map(menu);

        response.Categories.Should().HaveCount(1);
        response.Categories.First().Name.Should().Be("Appetizers");
    }

    #endregion

    #region DepositPolicyResponse.Map

    [Fact]
    public void DepositPolicyResponse_Map_MapsAllProperties()
    {
        var depositPolicy = new TestableDepositPolicy(
            DepositType.PercentageOfBill,
            amount: 100m,
            percentage: 25m,
            minimumBillForDeposit: 200m,
            minimumGuestsForDeposit: 6);

        var response = DepositPolicyResponse.Map(depositPolicy);

        response.DepositType.Should().Be(DepositType.PercentageOfBill);
        response.Amount.Should().Be(100m);
        response.Percentage.Should().Be(25m);
        response.MinimumBillForDeposit.Should().Be(200m);
        response.MinimumGuestsForDeposit.Should().Be(6);
    }

    #endregion

    #region MenuCategoryResponse.Map

    [Fact]
    public void MenuCategoryResponse_Map_MapsAllProperties()
    {
        var categoryId = Guid.NewGuid();
        var category = new TestableMenuCategory(categoryId)
        {
            Name = "Main Courses",
            Description = "Entrees",
            DisplayOrder = 2,
            IsActive = true
        };

        var response = MenuCategoryResponse.Map(category);

        response.Id.Should().Be(categoryId);
        response.Name.Should().Be("Main Courses");
        response.Description.Should().Be("Entrees");
        response.DisplayOrder.Should().Be(2);
        response.IsActive.Should().BeTrue();
        response.Items.Should().BeEmpty();
    }

    [Fact]
    public void MenuCategoryResponse_Map_WithItems_MapsItems()
    {
        var category = new TestableMenuCategory(Guid.NewGuid())
        {
            Name = "Appetizers",
            IsActive = true
        };

        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            Name = "Caesar Salad",
            IsActive = true,
            IsAvailable = true
        };

        var categoryItem = new TestableCategoryItem(menuItem, displayOrder: 1);
        category.AddItemDirect(categoryItem);

        var response = MenuCategoryResponse.Map(category);

        response.Items.Should().HaveCount(1);
        response.Items.First().MenuItem.Name.Should().Be("Caesar Salad");
    }

    #endregion

    #region CategoryItemResponse.Map

    [Fact]
    public void CategoryItemResponse_Map_MapsAllProperties()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            Name = "Steak",
            Description = "Grilled steak",
            ImageUrl = "https://example.com/steak.jpg",
            IsActive = true,
            IsAvailable = true
        };

        var categoryItem = new TestableCategoryItem(menuItem, displayOrder: 5);

        var response = CategoryItemResponse.Map(categoryItem);

        response.DisplayOrder.Should().Be(5);
        response.MenuItem.Should().NotBeNull();
        response.MenuItem.Name.Should().Be("Steak");
        response.PriceOverrides.Should().BeEmpty();
    }

    [Fact]
    public void CategoryItemResponse_Map_WithPriceOverrides_MapsPriceOverrides()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            Name = "Steak",
            IsActive = true
        };

        var priceOverrides = new HashSet<PriceOptionVO>
        {
            new TestablePriceOption(PortionType.Full, 25.99m),
            new TestablePriceOption(PortionType.Half, 15.99m)
        };

        var categoryItem = new TestableCategoryItem(menuItem, displayOrder: 1, priceOverrides);

        var response = CategoryItemResponse.Map(categoryItem);

        response.PriceOverrides.Should().HaveCount(2);
    }

    #endregion

    #region MenuItemSummaryResponse.Map

    [Fact]
    public void MenuItemSummaryResponse_Map_MapsAllProperties()
    {
        var itemId = Guid.NewGuid();
        var menuItem = new TestableMenuItem(itemId)
        {
            Name = "Caesar Salad",
            Description = "Fresh salad",
            ImageUrl = "https://example.com/salad.jpg",
            IsActive = true,
            IsAvailable = true
        };

        var response = MenuItemSummaryResponse.Map(menuItem);

        response.Id.Should().Be(itemId);
        response.Name.Should().Be("Caesar Salad");
        response.Description.Should().Be("Fresh salad");
        response.ImageUrl.Should().Be("https://example.com/salad.jpg");
        response.IsActive.Should().BeTrue();
        response.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void MenuItemSummaryResponse_Map_WithNullOptionalFields_MapsNullValues()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            Name = "Simple Item",
            IsActive = true
        };

        var response = MenuItemSummaryResponse.Map(menuItem);

        response.Description.Should().BeNull();
        response.ImageUrl.Should().BeNull();
    }

    #endregion
}
