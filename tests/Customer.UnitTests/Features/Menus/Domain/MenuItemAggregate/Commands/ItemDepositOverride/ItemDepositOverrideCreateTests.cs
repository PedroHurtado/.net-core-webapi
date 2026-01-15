namespace Customer.UnitTests.Features.Menus.Domain.MenuItemAggregate.Commands.ItemDepositOverride;

public class ItemDepositOverrideCreateTests
{
    private readonly ItemDepositOverrideValidator _validator = new();
    private readonly ItemDepositOverrideVO.Create _create;

    public ItemDepositOverrideCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsItemDepositOverride()
    {
        var command = new CreateItemDepositOverrideCommand(50.00m);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.DepositAmount.Should().Be(50.00m);
        result.MinimumQuantityForDeposit.Should().BeNull();
    }

    [Fact]
    public void Execute_WithMinimumQuantity_SetsMinimumQuantity()
    {
        var command = new CreateItemDepositOverrideCommand(100.00m, 5);

        var result = _create.Execute(command);

        result.DepositAmount.Should().Be(100.00m);
        result.MinimumQuantityForDeposit.Should().Be(5);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Execute_WithDifferentMinimumQuantities_SetsMinimumQuantity(int minimumQuantity)
    {
        var command = new CreateItemDepositOverrideCommand(50.00m, minimumQuantity);

        var result = _create.Execute(command);

        result.MinimumQuantityForDeposit.Should().Be(minimumQuantity);
    }

    #region Validation Throws

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Execute_WithZeroOrNegativeDepositAmount_ThrowsValidationException(decimal depositAmount)
    {
        var command = new CreateItemDepositOverrideCommand(depositAmount);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Execute_WithInvalidMinimumQuantity_ThrowsValidationException(int minimumQuantity)
    {
        var command = new CreateItemDepositOverrideCommand(50.00m, minimumQuantity);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}