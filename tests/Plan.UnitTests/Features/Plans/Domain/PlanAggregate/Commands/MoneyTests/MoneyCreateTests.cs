namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.MoneyTests;



public class MoneyCreateTests
{
    private readonly MoneyValidator _validator = new();
    private readonly Money.Create _create;

    public MoneyCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsMoney()
    {
        var command = new CreateMoneyCommand(10.00m, Currency.EUR);

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Amount.Should().Be(10.00m);
        result.Currency.Should().Be(Currency.EUR);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100.00)]
    public void Execute_WithDifferentAmounts_SetsCorrectAmount(decimal amount)
    {
        var command = new CreateMoneyCommand(amount, Currency.EUR);

        var result = _create.Execute(command);

        result.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Execute_WithDifferentCurrencies_SetsCorrectCurrency(string currencyCode)
    {
        var currency = Currency.FromCode(currencyCode);
        var command = new CreateMoneyCommand(10.00m, currency);

        var result = _create.Execute(command);

        result.Currency.Should().Be(currency);
    }

    [Fact]
    public void Execute_WithZeroAmount_ReturnsMoneyWithZero()
    {
        var command = new CreateMoneyCommand(0, Currency.USD);

        var result = _create.Execute(command);

        result.Amount.Should().Be(0);
        result.IsZero.Should().BeTrue();
    }

    #region Validation Throws

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-10.00)]
    [InlineData(-100.50)]
    public void Execute_WithNegativeAmount_ThrowsValidationException(decimal amount)
    {
        var command = new CreateMoneyCommand(amount, Currency.EUR);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNullCurrency_ThrowsValidationException()
    {
        var command = new CreateMoneyCommand(10.00m, null!);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
