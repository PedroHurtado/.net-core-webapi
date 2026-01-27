namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(99.99)]
    public void Amount_SetsCorrectValue(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        money.Amount.Should().Be(amount);
    }

    [Fact]
    public void Currency_SetsCorrectValue()
    {
        var currency = Currency.USD;
        var money = new TestableMoney(10.00m, currency);

        money.Currency.Should().Be(currency);
    }

    #region Zero

    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmount()
    {
        var money = Money.Zero(Currency.EUR);

        money.Amount.Should().Be(0);
        money.Currency.Should().Be(Currency.EUR);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void Zero_WithDifferentCurrencies_ReturnsCorrectCurrency(string currencyCode)
    {
        var currency = Currency.FromCode(currencyCode);
        var money = Money.Zero(currency);

        money.Currency.Should().Be(currency);
        money.Amount.Should().Be(0);
    }

    #endregion

    #region Add

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var money1 = new TestableMoney(10.00m, Currency.EUR);
        var money2 = new TestableMoney(5.50m, Currency.EUR);

        var result = money1.Add(money2);

        result.Amount.Should().Be(15.50m);
        result.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        var money1 = new TestableMoney(10.00m, Currency.EUR);
        var money2 = new TestableMoney(5.00m, Currency.USD);

        var act = () => money1.Add(money2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add money with different currencies*");
    }

    #endregion

    #region Subtract

    [Fact]
    public void Subtract_WithSameCurrency_ReturnsDifference()
    {
        var money1 = new TestableMoney(10.00m, Currency.EUR);
        var money2 = new TestableMoney(3.50m, Currency.EUR);

        var result = money1.Subtract(money2);

        result.Amount.Should().Be(6.50m);
        result.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Subtract_WithDifferentCurrencies_ThrowsInvalidOperationException()
    {
        var money1 = new TestableMoney(10.00m, Currency.EUR);
        var money2 = new TestableMoney(5.00m, Currency.USD);

        var act = () => money1.Subtract(money2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot subtract money with different currencies*");
    }

    #endregion

    #region Multiply

    [Theory]
    [InlineData(10.00, 2, 20.00)]
    [InlineData(10.00, 0.5, 5.00)]
    [InlineData(10.00, 0, 0)]
    public void Multiply_WithFactor_ReturnsMultipliedAmount(decimal amount, decimal factor, decimal expected)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        var result = money.Multiply(factor);

        result.Amount.Should().Be(expected);
        result.Currency.Should().Be(Currency.EUR);
    }

    #endregion

    #region IsZero

    [Fact]
    public void IsZero_WhenAmountIsZero_ReturnsTrue()
    {
        var money = new TestableMoney(0, Currency.EUR);

        money.IsZero.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.00)]
    [InlineData(-5.00)]
    public void IsZero_WhenAmountIsNotZero_ReturnsFalse(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        money.IsZero.Should().BeFalse();
    }

    #endregion

    #region IsPositive

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.00)]
    [InlineData(100.50)]
    public void IsPositive_WhenAmountIsPositive_ReturnsTrue(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        money.IsPositive.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5.00)]
    public void IsPositive_WhenAmountIsZeroOrNegative_ReturnsFalse(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        money.IsPositive.Should().BeFalse();
    }

    #endregion

    #region IsNegative

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-10.00)]
    [InlineData(-100.50)]
    public void IsNegative_WhenAmountIsNegative_ReturnsTrue(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        money.IsNegative.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5.00)]
    public void IsNegative_WhenAmountIsZeroOrPositive_ReturnsFalse(decimal amount)
    {
        var money = new TestableMoney(amount, Currency.EUR);

        money.IsNegative.Should().BeFalse();
    }

    #endregion
}
