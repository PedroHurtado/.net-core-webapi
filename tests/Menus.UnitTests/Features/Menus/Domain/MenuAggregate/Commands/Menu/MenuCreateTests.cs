namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.Menu;

public class MenuCreateTests
{
    private readonly MenuValidator _validator = new();
    private readonly MenuAgg.Create _create;

    public MenuCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsMenu()
    {
        var tenantId = Guid.NewGuid();
        var command = new CreateMenuCommand(tenantId, "Lunch Menu");

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Lunch Menu");
        result.Description.Should().BeNull();
        result.IsActive.Should().BeFalse();
        result.DisplayOrder.Should().Be(0);
        result.EffectiveFrom.Should().BeNull();
        result.EffectiveUntil.Should().BeNull();
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsOptionalFields()
    {
        var tenantId = Guid.NewGuid();
        var effectiveFrom = new DateTime(2024, 1, 1);
        var effectiveUntil = new DateTime(2024, 12, 31);
        var command = new CreateMenuCommand(
            TenantId: tenantId,
            Name: "Seasonal Menu",
            Description: "Special seasonal offerings",
            EffectiveFrom: effectiveFrom,
            EffectiveUntil: effectiveUntil);

        var result = _create.Execute(command);

        result.Description.Should().Be("Special seasonal offerings");
        result.EffectiveFrom.Should().Be(effectiveFrom);
        result.EffectiveUntil.Should().Be(effectiveUntil);
    }

    [Fact]
    public void Execute_WithOnlyEffectiveFrom_SetsEffectiveFrom()
    {
        var effectiveFrom = new DateTime(2024, 6, 1);
        var command = new CreateMenuCommand(Guid.NewGuid(), "Summer Menu", EffectiveFrom: effectiveFrom);

        var result = _create.Execute(command);

        result.EffectiveFrom.Should().Be(effectiveFrom);
        result.EffectiveUntil.Should().BeNull();
    }

    [Fact]
    public void Execute_WithOnlyEffectiveUntil_SetsEffectiveUntil()
    {
        var effectiveUntil = new DateTime(2024, 8, 31);
        var command = new CreateMenuCommand(Guid.NewGuid(), "Limited Menu", EffectiveUntil: effectiveUntil);

        var result = _create.Execute(command);

        result.EffectiveFrom.Should().BeNull();
        result.EffectiveUntil.Should().Be(effectiveUntil);
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithEmptyTenantId_ThrowsValidationException()
    {
        var command = new CreateMenuCommand(Guid.Empty, "Menu");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var command = new CreateMenuCommand(Guid.NewGuid(), name!);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreateMenuCommand(Guid.NewGuid(), new string('a', 101));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreateMenuCommand(Guid.NewGuid(), "Menu", Description: new string('a', 501));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEffectiveFromAfterEffectiveUntil_ThrowsValidationException()
    {
        var command = new CreateMenuCommand(
            Guid.NewGuid(),
            "Menu",
            EffectiveFrom: new DateTime(2024, 12, 31),
            EffectiveUntil: new DateTime(2024, 1, 1));

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithSameEffectiveFromAndUntil_ThrowsValidationException()
    {
        var sameDate = new DateTime(2024, 6, 15);
        var command = new CreateMenuCommand(
            Guid.NewGuid(),
            "Menu",
            EffectiveFrom: sameDate,
            EffectiveUntil: sameDate);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}