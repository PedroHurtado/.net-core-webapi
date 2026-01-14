using FluentAssertions;
using Schedule.Features.Menu.Models;
using Xunit;

namespace ScheDule.UnitTests.Features.Menu;

public class MenuTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var name = "Main Menu";
        var description = "Our main offerings";
        
        // Act
        var result = Schedule.Features.Menu.Models.Menu.Create(id, restaurantId, name, description);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.RestaurantId.Should().Be(restaurantId);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Act
        var result = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "");
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Create_WithInvalidDates_ShouldReturnFailure()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow.AddDays(1);
        var effectiveUntil = DateTime.UtcNow; // Before From
        
        // Act
        var result = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu", null, effectiveFrom, effectiveUntil);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "EffectiveFrom");
    }

    [Fact]
    public void SetDepositPolicy_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        
        // Act
        var result = menu.SetDepositPolicy(DepositType.PerPerson, 10m, null, null, 6);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.DepositPolicy.Should().NotBeNull();
        menu.DepositPolicy!.Amount.Should().Be(10m);
        menu.DepositPolicy.MinimumGuestsForDeposit.Should().Be(6);
    }

    [Fact]
    public void SetDepositPolicy_WithInvalidPercentage_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        
        // Act
        var result = menu.SetDepositPolicy(DepositType.PercentageOfBill, 0m, 150m); // > 100%
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "percentage");
    }

    [Fact]
    public void AddCategory_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        
        // Act
        var result = menu.AddCategory("Starters", "Beginnings");
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.Categories.Should().ContainSingle(c => c.Name == "Starters");
    }

    [Fact]
    public void AddCategory_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.AddCategory("Starters");
        
        // Act
        var result = menu.AddCategory("Starters");
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "name");
    }

    [Fact]
    public void AddItem_ToCategory_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.AddCategory("Starters");
        var category = menu.Categories.First();
        var priceOptions = new List<PriceOption> { new PriceOption(Guid.NewGuid(), PortionType.Racion, 10m) };

        // Act
        var result = category.AddItem("Soup", "Hot", priceOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Items.Should().ContainSingle(i => i.Name == "Soup");
    }

    [Fact]
    public void SetDepositPolicy_WithPercentageOnNonPercentageType_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        var result = menu.SetDepositPolicy(DepositType.PerPerson, 10m, 50m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "percentage");
    }

    [Fact]
    public void SetDepositPolicy_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        var result = menu.SetDepositPolicy(DepositType.FixedAmount, 0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "amount");
    }

    [Fact]
    public void RemoveDepositPolicy_ShouldClearPolicy()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.SetDepositPolicy(DepositType.PerPerson, 10m);

        // Act
        var result = menu.RemoveDepositPolicy();

        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.DepositPolicy.Should().BeNull();
    }

    [Fact]
    public void AddCategory_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        var result = menu.AddCategory("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "name");
    }

    [Fact]
    public void AddCategory_WithNameExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        var longName = new string('A', 101);

        // Act
        var result = menu.AddCategory(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "name");
    }

    [Fact]
    public void AddItem_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.AddCategory("Starters");
        var category = menu.Categories.First();
        var priceOptions = new List<PriceOption> { new PriceOption(Guid.NewGuid(), PortionType.Racion, 10m) };
        category.AddItem("Soup", "Hot", priceOptions);

        // Act
        var result = category.AddItem("Soup", "Different description", priceOptions);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "name");
    }

    [Fact]
    public void AddItem_WithEmptyPriceOptions_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.AddCategory("Starters");
        var category = menu.Categories.First();

        // Act
        var result = category.AddItem("Soup", "Hot", new List<PriceOption>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "priceOptions");
    }

    [Fact]
    public void AddItem_WithNullPriceOptions_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.AddCategory("Starters");
        var category = menu.Categories.First();

        // Act
        var result = category.AddItem("Soup", "Hot", null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "priceOptions");
    }

    [Fact]
    public void MenuItem_ShouldHaveCorrectDefaultValues()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;
        menu.AddCategory("Starters");
        var category = menu.Categories.First();
        var priceOptions = new List<PriceOption> { new PriceOption(Guid.NewGuid(), PortionType.Racion, 10m) };
        category.AddItem("Soup", "Hot soup", priceOptions);

        // Act
        var item = category.Items.First();

        // Assert
        item.IsActive.Should().BeTrue();
        item.IsAvailable.Should().BeTrue();
        item.Name.Should().Be("Soup");
        item.Description.Should().Be("Hot soup");
        item.PriceOptions.Should().HaveCount(1);
    }

    [Fact]
    public void MenuCategory_ShouldHaveCorrectDefaultValues()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        menu.AddCategory("Starters", "First courses");
        var category = menu.Categories.First();

        // Assert
        category.IsActive.Should().BeTrue();
        category.Name.Should().Be("Starters");
        category.Description.Should().Be("First courses");
        category.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void SetDepositPolicy_PercentageOfBill_WithoutPercentage_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        var result = menu.SetDepositPolicy(DepositType.PercentageOfBill, 0m, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "percentage");
    }

    [Fact]
    public void SetDepositPolicy_PercentageOfBill_WithPercentageLessThanOne_ShouldReturnFailure()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        var result = menu.SetDepositPolicy(DepositType.PercentageOfBill, 0m, 0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.PropertyName == "percentage");
    }

    [Fact]
    public void SetDepositPolicy_PercentageOfBill_WithValidPercentage_ShouldReturnSuccess()
    {
        // Arrange
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Menu").Value!;

        // Act
        var result = menu.SetDepositPolicy(DepositType.PercentageOfBill, 0m, 50m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.DepositPolicy.Should().NotBeNull();
        menu.DepositPolicy!.Percentage.Should().Be(50m);
    }

    [Fact]
    public void Menu_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var menu = Schedule.Features.Menu.Models.Menu.Create(Guid.NewGuid(), Guid.NewGuid(), "Test Menu", "Description").Value!;

        // Assert
        menu.DisplayOrder.Should().Be(0);
        menu.Description.Should().Be("Description");
    }
}
