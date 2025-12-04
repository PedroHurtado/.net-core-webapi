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
}
