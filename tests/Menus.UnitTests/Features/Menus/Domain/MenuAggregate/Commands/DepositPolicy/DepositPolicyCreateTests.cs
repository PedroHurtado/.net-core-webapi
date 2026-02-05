namespace Menus.UnitTests.Features.Menus.Domain.MenuAggregate.Commands.DepositPolicy;

public class DepositPolicyCreateTests
{
    private readonly DepositPolicyValidator _validator = new();
    private readonly DepositPolicyVO.Create _create;

    public DepositPolicyCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidFixedAmount_ReturnsDepositPolicy()
    {
        var command = new CreateDepositPolicyCommand(DepositType.FixedAmount, 50m);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.DepositType.Should().Be(DepositType.FixedAmount);
        result.Amount.Should().Be(50m);
        result.Percentage.Should().BeNull();
        result.MinimumBillForDeposit.Should().BeNull();
        result.MinimumGuestsForDeposit.Should().BeNull();
    }

    [Fact]
    public void Execute_WithValidPerPerson_ReturnsDepositPolicy()
    {
        var command = new CreateDepositPolicyCommand(DepositType.PerPerson, 10m);

        var result = _create.Execute(command);

        result.DepositType.Should().Be(DepositType.PerPerson);
        result.Amount.Should().Be(10m);
    }

    [Fact]
    public void Execute_WithValidPercentageOfBill_ReturnsDepositPolicy()
    {
        var command = new CreateDepositPolicyCommand(DepositType.PercentageOfBill, 1m, Percentage: 25m);

        var result = _create.Execute(command);

        result.DepositType.Should().Be(DepositType.PercentageOfBill);
        result.Percentage.Should().Be(25m);
    }

    [Fact]
    public void Execute_WithOptionalFields_SetsOptionalFields()
    {
        var command = new CreateDepositPolicyCommand(
            DepositType: DepositType.FixedAmount,
            Amount: 50m,
            MinimumBillForDeposit: 200m,
            MinimumGuestsForDeposit: 5);

        var result = _create.Execute(command);

        result.MinimumBillForDeposit.Should().Be(200m);
        result.MinimumGuestsForDeposit.Should().Be(5);
    }

    #region Validation Throws

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Execute_WithZeroOrNegativeAmount_ThrowsValidationException(decimal amount)
    {
        var command = new CreateDepositPolicyCommand(DepositType.FixedAmount, amount);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithPercentageOfBillAndNullPercentage_ThrowsValidationException()
    {
        var command = new CreateDepositPolicyCommand(DepositType.PercentageOfBill, 1m, Percentage: null);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(DepositType.FixedAmount)]
    [InlineData(DepositType.PerPerson)]
    public void Execute_WithNonPercentageTypeAndPercentage_ThrowsValidationException(DepositType depositType)
    {
        var command = new CreateDepositPolicyCommand(depositType, 50m, Percentage: 25m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Execute_WithPercentageOutOfRange_ThrowsValidationException(decimal percentage)
    {
        var command = new CreateDepositPolicyCommand(DepositType.PercentageOfBill, 1m, Percentage: percentage);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Execute_WithMinimumGuestsLessThanOne_ThrowsValidationException(int minimumGuests)
    {
        var command = new CreateDepositPolicyCommand(DepositType.FixedAmount, 50m, MinimumGuestsForDeposit: minimumGuests);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeMinimumBill_ThrowsValidationException()
    {
        var command = new CreateDepositPolicyCommand(DepositType.FixedAmount, 50m, MinimumBillForDeposit: -1m);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
