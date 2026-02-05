using Menus.Features.Menus.Api.MenuItemAggregate;

namespace Menus.UnitTests.Features.Menus.Api.MenuItemAggregate;

public class MenuItemResponseTests
{
    #region MenuItemResponse.Map Tests

    [Fact]
    public void Map_WithAllProperties_MapsCorrectly()
    {
        // Arrange
        var depositOverride = new TestableItemDepositOverride(30.00m, 4);
        var nutritionalInfo = new TestableNutritionalInfo(600, 45m, 2m, 45m, 200);
        var priceOption = new TestablePriceOption(PortionType.Full, 22.00m);
        var allergen = new TestableAllergen("GLUTEN") { Name = "Gluten", IconUrl = "https://example.com/gluten.png" };

        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Pulpo al Horno",
            Description = "Pulpo gallego con pimentón",
            ImageUrl = "https://example.com/pulpo.jpg",
            DisplayOrder = 1,
            IsActive = true,
            IsAvailable = true,
            IsHighRiskItem = true,
            RequiresAdvanceOrder = true,
            MinimumAdvanceOrderQuantity = 4,
            IsAlwaysAvailable = false,
            AllergenNotes = "Puede contener trazas",
            DepositOverride = depositOverride,
            NutritionalInfo = nutritionalInfo
        };
        menuItem.AddPriceOptionDirect(priceOption);
        menuItem.AddAllergenDirect(allergen);
        menuItem.AddAvailableDay(DayOfWeek.Friday);
        menuItem.AddAvailableDay(DayOfWeek.Saturday);

        // Act
        var response = MenuItemResponse.Map(menuItem);

        // Assert
        response.Id.Should().Be(menuItem.Id);
        response.Name.Should().Be("Pulpo al Horno");
        response.Description.Should().Be("Pulpo gallego con pimentón");
        response.ImageUrl.Should().Be("https://example.com/pulpo.jpg");
        response.DisplayOrder.Should().Be(1);
        response.IsActive.Should().BeTrue();
        response.IsAvailable.Should().BeTrue();
        response.IsHighRiskItem.Should().BeTrue();
        response.RequiresAdvanceOrder.Should().BeTrue();
        response.MinimumAdvanceOrderQuantity.Should().Be(4);
        response.IsAlwaysAvailable.Should().BeFalse();
        response.AllergenNotes.Should().Be("Puede contener trazas");
    }

    [Fact]
    public void Map_MapsComputedProperties()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.Full, 22.00m, isActive: true);
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Test Item",
            IsActive = true,
            IsAvailable = true,
            IsAlwaysAvailable = true,
            DepositOverride = new TestableItemDepositOverride(30.00m)
        };
        menuItem.AddPriceOptionDirect(priceOption);

        // Act
        var response = MenuItemResponse.Map(menuItem);

        // Assert
        response.IsAvailableToday.Should().Be(menuItem.IsAvailableToday);
        response.CanBeOrdered.Should().Be(menuItem.CanBeOrdered);
        response.HasDepositOverride.Should().BeTrue();
    }

    [Fact]
    public void Map_WithNullOptionalProperties_MapsNullCorrectly()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.Full, 22.00m);
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Simple Item",
            Description = null,
            ImageUrl = null,
            AllergenNotes = null,
            DepositOverride = null,
            NutritionalInfo = null,
            IsAlwaysAvailable = true
        };
        menuItem.AddPriceOptionDirect(priceOption);

        // Act
        var response = MenuItemResponse.Map(menuItem);

        // Assert
        response.Description.Should().BeNull();
        response.ImageUrl.Should().BeNull();
        response.AllergenNotes.Should().BeNull();
        response.DepositOverride.Should().BeNull();
        response.NutritionalInfo.Should().BeNull();
        response.HasDepositOverride.Should().BeFalse();
    }

    [Fact]
    public void Map_WithMultiplePriceOptions_MapsAllOptions()
    {
        // Arrange
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Multi Price Item",
            IsAlwaysAvailable = true
        };
        menuItem.AddPriceOptionDirect(new TestablePriceOption(PortionType.Small, 3.50m));
        menuItem.AddPriceOptionDirect(new TestablePriceOption(PortionType.Half, 7.00m));
        menuItem.AddPriceOptionDirect(new TestablePriceOption(PortionType.Full, 14.00m));

        // Act
        var response = MenuItemResponse.Map(menuItem);

        // Assert
        response.PriceOptions.Should().HaveCount(3);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Small && p.Price == 3.50m);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Half && p.Price == 7.00m);
        response.PriceOptions.Should().Contain(p => p.PortionType == PortionType.Full && p.Price == 14.00m);
    }

    [Fact]
    public void Map_WithMultipleAllergens_MapsAllAllergens()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.Full, 10.00m);
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Multi Allergen Item",
            IsAlwaysAvailable = true
        };
        menuItem.AddPriceOptionDirect(priceOption);
        menuItem.AddAllergenDirect(new TestableAllergen("GLUTEN") { Name = "Gluten" });
        menuItem.AddAllergenDirect(new TestableAllergen("LACTEOS") { Name = "Lácteos" });

        // Act
        var response = MenuItemResponse.Map(menuItem);

        // Assert
        response.Allergens.Should().HaveCount(2);
        response.Allergens.Should().Contain(a => a.Id == "GLUTEN" && a.Name == "Gluten");
        response.Allergens.Should().Contain(a => a.Id == "LACTEOS" && a.Name == "Lácteos");
    }

    [Fact]
    public void Map_WithAvailableDays_MapsAllDays()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.Full, 10.00m);
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            TenantId = Guid.NewGuid(),
            Name = "Weekend Item",
            IsAlwaysAvailable = false
        };
        menuItem.AddPriceOptionDirect(priceOption);
        menuItem.AddAvailableDay(DayOfWeek.Friday);
        menuItem.AddAvailableDay(DayOfWeek.Saturday);
        menuItem.AddAvailableDay(DayOfWeek.Sunday);

        // Act
        var response = MenuItemResponse.Map(menuItem);

        // Assert
        response.AvailableDays.Should().HaveCount(3);
        response.AvailableDays.Should().Contain(DayOfWeek.Friday);
        response.AvailableDays.Should().Contain(DayOfWeek.Saturday);
        response.AvailableDays.Should().Contain(DayOfWeek.Sunday);
    }

    #endregion

    #region PriceOptionResponse.Map Tests

    [Fact]
    public void PriceOptionMap_WithFixedPrice_MapsCorrectly()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.Full, 14.00m, isActive: true);

        // Act
        var response = PriceOptionResponse.Map(priceOption);

        // Assert
        response.PortionType.Should().Be(PortionType.Full);
        response.Price.Should().Be(14.00m);
        response.IsActive.Should().BeTrue();
        response.RequiresMarketPrice.Should().BeFalse();
        response.DisplayPrice.Should().Be(priceOption.DisplayPrice);
    }

    [Fact]
    public void PriceOptionMap_WithMarketPriceNoValue_MapsCorrectly()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, null);

        // Act
        var response = PriceOptionResponse.Map(priceOption);

        // Assert
        response.PortionType.Should().Be(PortionType.MarketPrice);
        response.Price.Should().BeNull();
        response.RequiresMarketPrice.Should().BeTrue();
        response.DisplayPrice.Should().Be("S/M");
    }

    [Fact]
    public void PriceOptionMap_WithMarketPriceWithValue_MapsCorrectly()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.MarketPrice, 22.00m);

        // Act
        var response = PriceOptionResponse.Map(priceOption);

        // Assert
        response.PortionType.Should().Be(PortionType.MarketPrice);
        response.Price.Should().Be(22.00m);
        response.RequiresMarketPrice.Should().BeFalse();
    }

    [Fact]
    public void PriceOptionMap_WithInactiveOption_MapsCorrectly()
    {
        // Arrange
        var priceOption = new TestablePriceOption(PortionType.Half, 7.00m, isActive: false);

        // Act
        var response = PriceOptionResponse.Map(priceOption);

        // Assert
        response.IsActive.Should().BeFalse();
    }

    #endregion

    #region ItemDepositOverrideResponse.Map Tests

    [Fact]
    public void ItemDepositOverrideMap_WithoutMinimumQuantity_MapsCorrectly()
    {
        // Arrange
        var depositOverride = new TestableItemDepositOverride(30.00m);

        // Act
        var response = ItemDepositOverrideResponse.Map(depositOverride);

        // Assert
        response.DepositAmount.Should().Be(30.00m);
        response.MinimumQuantityForDeposit.Should().BeNull();
        response.AppliesToAllQuantities.Should().BeTrue();
    }

    [Fact]
    public void ItemDepositOverrideMap_WithMinimumQuantity_MapsCorrectly()
    {
        // Arrange
        var depositOverride = new TestableItemDepositOverride(30.00m, 4);

        // Act
        var response = ItemDepositOverrideResponse.Map(depositOverride);

        // Assert
        response.DepositAmount.Should().Be(30.00m);
        response.MinimumQuantityForDeposit.Should().Be(4);
        response.AppliesToAllQuantities.Should().BeFalse();
    }

    #endregion

    #region NutritionalInfoResponse.Map Tests

    [Fact]
    public void NutritionalInfoMap_WithAllValues_MapsCorrectly()
    {
        // Arrange
        var nutritionalInfo = new TestableNutritionalInfo(
            calories: 600,
            protein: 45m,
            carbohydrates: 2m,
            fat: 45m,
            servingSize: 200,
            fiber: 2.5m,
            sugar: 1m,
            salt: 3.2m);

        // Act
        var response = NutritionalInfoResponse.Map(nutritionalInfo);

        // Assert
        response.Calories.Should().Be(600);
        response.Protein.Should().Be(45m);
        response.Carbohydrates.Should().Be(2m);
        response.Fat.Should().Be(45m);
        response.ServingSize.Should().Be(200);
        response.Fiber.Should().Be(2.5m);
        response.Sugar.Should().Be(1m);
        response.Salt.Should().Be(3.2m);
    }

    [Fact]
    public void NutritionalInfoMap_WithOnlyRequiredValues_MapsCorrectly()
    {
        // Arrange
        var nutritionalInfo = new TestableNutritionalInfo(
            calories: 180,
            protein: 8m,
            carbohydrates: 15m,
            fat: 9m,
            servingSize: 300);

        // Act
        var response = NutritionalInfoResponse.Map(nutritionalInfo);

        // Assert
        response.Calories.Should().Be(180);
        response.Protein.Should().Be(8m);
        response.Carbohydrates.Should().Be(15m);
        response.Fat.Should().Be(9m);
        response.ServingSize.Should().Be(300);
        response.Fiber.Should().BeNull();
        response.Sugar.Should().BeNull();
        response.Salt.Should().BeNull();
    }

    #endregion

    #region AllergenResponse.Map Tests

    [Fact]
    public void AllergenMap_WithAllProperties_MapsCorrectly()
    {
        // Arrange
        var allergen = new TestableAllergen("GLUTEN")
        {
            Name = "Gluten",
            IconUrl = "https://example.com/gluten.png"
        };

        // Act
        var response = AllergenResponse.Map(allergen);

        // Assert
        response.Id.Should().Be("GLUTEN");
        response.Name.Should().Be("Gluten");
        response.IconUrl.Should().Be("https://example.com/gluten.png");
    }

    [Fact]
    public void AllergenMap_WithNullIconUrl_MapsCorrectly()
    {
        // Arrange
        var allergen = new TestableAllergen("LACTEOS")
        {
            Name = "Lácteos",
            IconUrl = null
        };

        // Act
        var response = AllergenResponse.Map(allergen);

        // Assert
        response.Id.Should().Be("LACTEOS");
        response.Name.Should().Be("Lácteos");
        response.IconUrl.Should().BeNull();
    }

    #endregion
}
