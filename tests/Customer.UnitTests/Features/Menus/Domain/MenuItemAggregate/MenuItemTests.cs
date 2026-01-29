namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate;

public class MenuItemTests
{
    private readonly PriceOptionVO.Create _createPriceOption = new(new PriceOptionValidator());
    private readonly ItemDepositOverrideVO.Create _createDepositOverride = new(new ItemDepositOverrideValidator());
    private readonly NutritionalInfoVO.Create _createNutritionalInfo = new(new NutritionalInfoValidator());
    private readonly Allergen.Create _createAllergen = new(new AllergenValidator());

    [Fact]
    public void ParameterlessConstructor_InitializesWithEmptyId()
    {
        var menuItem = new TestableMenuItem();

        menuItem.Id.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithId_SetsCorrectValue()
    {
        var id = Guid.NewGuid();

        var menuItem = new TestableMenuItem(id);

        menuItem.Id.Should().Be(id);
    }

    [Fact]
    public void TenantId_SetsCorrectValue()
    {
        var tenantId = Guid.NewGuid();
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { TenantId = tenantId };

        menuItem.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Name_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { Name = "Paella Valenciana" };

        menuItem.Name.Should().Be("Paella Valenciana");
    }

    [Fact]
    public void Description_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { Description = "Traditional rice dish" };

        menuItem.Description.Should().Be("Traditional rice dish");
    }

    [Fact]
    public void ImageUrl_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { ImageUrl = "https://example.com/paella.jpg" };

        menuItem.ImageUrl.Should().Be("https://example.com/paella.jpg");
    }

    [Fact]
    public void DisplayOrder_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { DisplayOrder = 5 };

        menuItem.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void IsActive_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { IsActive = true };

        menuItem.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsHighRiskItem_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { IsHighRiskItem = true };

        menuItem.IsHighRiskItem.Should().BeTrue();
    }

    [Fact]
    public void RequiresAdvanceOrder_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { RequiresAdvanceOrder = true };

        menuItem.RequiresAdvanceOrder.Should().BeTrue();
    }

    [Fact]
    public void MinimumAdvanceOrderQuantity_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { MinimumAdvanceOrderQuantity = 10 };

        menuItem.MinimumAdvanceOrderQuantity.Should().Be(10);
    }

    [Fact]
    public void IsAvailable_DefaultsToTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsAlwaysAvailable_DefaultsToTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.IsAlwaysAvailable.Should().BeTrue();
    }

    [Fact]
    public void AllergenNotes_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { AllergenNotes = "May contain traces of nuts" };

        menuItem.AllergenNotes.Should().Be("May contain traces of nuts");
    }

    #region Value Objects

    [Fact]
    public void DepositOverride_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        var depositOverride = _createDepositOverride.Execute(new CreateItemDepositOverrideCommand(25.00m, 5));

        menuItem.DepositOverride = depositOverride;

        menuItem.DepositOverride.Should().Be(depositOverride);
        menuItem.DepositOverride!.DepositAmount.Should().Be(25.00m);
        menuItem.DepositOverride.MinimumQuantityForDeposit.Should().Be(5);
    }

    [Fact]
    public void DepositOverride_DefaultsToNull()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.DepositOverride.Should().BeNull();
    }

    [Fact]
    public void NutritionalInfo_SetsCorrectValue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        var nutritionalInfo = _createNutritionalInfo.Execute(new CreateNutritionalInfoCommand(
            Calories: 500,
            Protein: 25.0m,
            Carbohydrates: 60.0m,
            Fat: 15.0m,
            ServingSize: 350));

        menuItem.NutritionalInfo = nutritionalInfo;

        menuItem.NutritionalInfo.Should().Be(nutritionalInfo);
        menuItem.NutritionalInfo!.Calories.Should().Be(500);
        menuItem.NutritionalInfo.Protein.Should().Be(25.0m);
        menuItem.NutritionalInfo.Carbohydrates.Should().Be(60.0m);
        menuItem.NutritionalInfo.Fat.Should().Be(15.0m);
        menuItem.NutritionalInfo.ServingSize.Should().Be(350);
    }

    [Fact]
    public void NutritionalInfo_DefaultsToNull()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.NutritionalInfo.Should().BeNull();
    }

    #endregion

    #region Computed Properties

    [Fact]
    public void IsAvailableToday_WhenAlwaysAvailable_ReturnsTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { IsAlwaysAvailable = true };

        menuItem.IsAvailableToday.Should().BeTrue();
    }

    [Fact]
    public void IsAvailableToday_WhenTodayIsInAvailableDays_ReturnsTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { IsAlwaysAvailable = false };
        menuItem.AddAvailableDay(DateTime.Today.DayOfWeek);

        menuItem.IsAvailableToday.Should().BeTrue();
    }

    [Fact]
    public void IsAvailableToday_WhenTodayIsNotInAvailableDays_ReturnsFalse()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid()) { IsAlwaysAvailable = false };
        var tomorrow = DateTime.Today.AddDays(1).DayOfWeek;
        menuItem.AddAvailableDay(tomorrow);

        menuItem.IsAvailableToday.Should().BeFalse();
    }

    [Fact]
    public void CanBeOrdered_WhenActiveAndAvailableTodayAndAvailable_ReturnsTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            IsActive = true,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        menuItem.CanBeOrdered.Should().BeTrue();
    }

    [Fact]
    public void CanBeOrdered_WhenNotActive_ReturnsFalse()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            IsActive = false,
            IsAvailable = true,
            IsAlwaysAvailable = true
        };

        menuItem.CanBeOrdered.Should().BeFalse();
    }

    [Fact]
    public void CanBeOrdered_WhenNotAvailable_ReturnsFalse()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid())
        {
            IsActive = true,
            IsAvailable = false,
            IsAlwaysAvailable = true
        };

        menuItem.CanBeOrdered.Should().BeFalse();
    }

    [Fact]
    public void HasDepositOverride_WhenDepositOverrideIsSet_ReturnsTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        menuItem.DepositOverride = _createDepositOverride.Execute(new CreateItemDepositOverrideCommand(25.00m));

        menuItem.HasDepositOverride.Should().BeTrue();
    }

    [Fact]
    public void HasDepositOverride_WhenDepositOverrideIsNull_ReturnsFalse()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.HasDepositOverride.Should().BeFalse();
    }

    [Fact]
    public void HasActivePriceOption_WhenHasActivePriceOption_ReturnsTrue()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        menuItem.AddPriceOptionDirect(_createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.99m, true)));

        menuItem.HasActivePriceOption.Should().BeTrue();
    }

    [Fact]
    public void HasActivePriceOption_WhenAllPriceOptionsInactive_ReturnsFalse()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        menuItem.AddPriceOptionDirect(_createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.99m, false)));

        menuItem.HasActivePriceOption.Should().BeFalse();
    }

    [Fact]
    public void HasActivePriceOption_WhenNoPriceOptions_ReturnsFalse()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.HasActivePriceOption.Should().BeFalse();
    }

    #endregion

    #region Collections

    [Fact]
    public void PriceOptions_ReturnsAddedOptions()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        var priceOption = _createPriceOption.Execute(new CreatePriceOptionCommand(PortionType.Full, 15.99m));

        menuItem.AddPriceOptionDirect(priceOption);

        menuItem.PriceOptions.Should().ContainSingle()
            .Which.Should().Be(priceOption);
    }

    [Fact]
    public void Allergens_ReturnsAddedAllergens()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());
        var allergen = _createAllergen.Execute(new CreateAllergenCommand("GLUTEN", "Gluten"));

        menuItem.AddAllergenDirect(allergen);

        menuItem.Allergens.Should().ContainSingle()
            .Which.Should().Be(allergen);
    }

    [Fact]
    public void AvailableDays_ReturnsAddedDays()
    {
        var menuItem = new TestableMenuItem(Guid.NewGuid());

        menuItem.AddAvailableDay(DayOfWeek.Monday);
        menuItem.AddAvailableDay(DayOfWeek.Wednesday);

        menuItem.AvailableDays.Should().HaveCount(2);
        menuItem.AvailableDays.Should().Contain(DayOfWeek.Monday);
        menuItem.AvailableDays.Should().Contain(DayOfWeek.Wednesday);
    }

    #endregion
}