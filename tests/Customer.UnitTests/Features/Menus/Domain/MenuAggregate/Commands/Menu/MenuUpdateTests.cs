namespace Customer.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuUpdateTests
{
    private readonly MenuValidator _validator = new();
    private readonly MenuAgg.Update _update;

    public MenuUpdateTests()
    {
        _update = new(_validator);
    }

    private static TestableMenu CreateValidMenu() => new(Guid.NewGuid())
    {
        TenantId = Guid.NewGuid(),
        Name = "Original Menu",
        Description = "Original description",
        IsActive = true,
        DisplayOrder = 0
    };

    [Fact]
    public void Execute_WithValidCommand_UpdatesMenu()
    {
        var menu = CreateValidMenu();
        var command = new UpdateMenuCommand(
            Name: "Updated Menu",
            Description: "Updated description",
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: 5);

        var result = _update.Execute(menu, command);

        result.Name.Should().Be("Updated Menu");
        result.Description.Should().Be("Updated description");
        result.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void Execute_WithEffectiveDates_SetsEffectiveDates()
    {
        var menu = CreateValidMenu();
        var effectiveFrom = new DateTime(2024, 1, 1);
        var effectiveUntil = new DateTime(2024, 12, 31);
        var command = new UpdateMenuCommand(
            Name: "Seasonal Menu",
            Description: null,
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil,
            DisplayOrder: 0);

        var result = _update.Execute(menu, command);

        result.EffectiveFrom.Should().Be(effectiveFrom);
        result.EffectiveUntil.Should().Be(effectiveUntil);
    }

    [Fact]
    public void Execute_WithNullDescription_ClearsDescription()
    {
        var menu = CreateValidMenu();
        menu.Description.Should().NotBeNull();
        var command = new UpdateMenuCommand(
            Name: "Menu",
            Description: null,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: 0);

        var result = _update.Execute(menu, command);

        result.Description.Should().BeNull();
    }

    [Fact]
    public void Execute_PreservesNonUpdatedProperties()
    {
        var menu = CreateValidMenu();
        var originalTenantId = menu.TenantId;
        var originalId = menu.Id;
        var originalIsActive = menu.IsActive;
        var command = new UpdateMenuCommand(
            Name: "New Name",
            Description: "New description",
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: 1);

        var result = _update.Execute(menu, command);

        result.Id.Should().Be(originalId);
        result.TenantId.Should().Be(originalTenantId);
        result.IsActive.Should().Be(originalIsActive);
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var menu = CreateValidMenu();
        var command = new UpdateMenuCommand(
            Name: name!,
            Description: null,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: 0);

        var act = () => _update.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new UpdateMenuCommand(
            Name: new string('a', 101),
            Description: null,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: 0);

        var act = () => _update.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new UpdateMenuCommand(
            Name: "Menu",
            Description: new string('a', 501),
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: 0);

        var act = () => _update.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeDisplayOrder_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new UpdateMenuCommand(
            Name: "Menu",
            Description: null,
            EffectiveFrom: null,
            EffectiveUntil: null,
            DisplayOrder: -1);

        var act = () => _update.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEffectiveFromAfterEffectiveUntil_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var command = new UpdateMenuCommand(
            Name: "Menu",
            Description: null,
            EffectiveFrom: new DateTime(2024, 12, 31),
            EffectiveUntil: new DateTime(2024, 1, 1),
            DisplayOrder: 0);

        var act = () => _update.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithSameEffectiveFromAndUntil_ThrowsValidationException()
    {
        var menu = CreateValidMenu();
        var sameDate = new DateTime(2024, 6, 15);
        var command = new UpdateMenuCommand(
            Name: "Menu",
            Description: null,
            EffectiveFrom: sameDate,
            EffectiveUntil: sameDate,
            DisplayOrder: 0);

        var act = () => _update.Execute(menu, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}