namespace Subscriptions.UnitTests.DomainTests.SharedTests.ValueObjectsTests;

public class MoneyTests
{
    #region Amount

    [Theory]
    [InlineData(9.99)]
    [InlineData(0)]
    [InlineData(100.50)]
    public void Amount_SetsCorrectValue(decimal amount)
    {
        var vo = new Money(amount, Currency.EUR);

        vo.Amount.Should().Be(amount);
    }

    #endregion

    #region Currency

    [Fact]
    public void Currency_SetsCorrectValue()
    {
        var currency = Currency.USD;

        var vo = new Money(9.99m, currency);

        vo.Currency.Should().Be(currency);
    }

    #endregion

    #region IsZero

    [Fact]
    public void IsZero_WhenAmountIsZero_ShouldReturnTrue()
    {
        var vo = new Money(0m, Currency.EUR);

        vo.IsZero.Should().BeTrue();
    }

    [Fact]
    public void IsZero_WhenAmountIsPositive_ShouldReturnFalse()
    {
        var vo = new Money(9.99m, Currency.EUR);

        vo.IsZero.Should().BeFalse();
    }

    #endregion

    #region IsPositive

    [Fact]
    public void IsPositive_WhenAmountIsPositive_ShouldReturnTrue()
    {
        var vo = new Money(9.99m, Currency.EUR);

        vo.IsPositive.Should().BeTrue();
    }

    [Fact]
    public void IsPositive_WhenAmountIsZero_ShouldReturnFalse()
    {
        var vo = new Money(0m, Currency.EUR);

        vo.IsPositive.Should().BeFalse();
    }

    #endregion

    #region IsNegative

    [Fact]
    public void IsNegative_WhenAmountIsNegative_ShouldReturnTrue()
    {
        var vo = new Money(-5m, Currency.EUR);

        vo.IsNegative.Should().BeTrue();
    }

    [Fact]
    public void IsNegative_WhenAmountIsPositive_ShouldReturnFalse()
    {
        var vo = new Money(9.99m, Currency.EUR);

        vo.IsNegative.Should().BeFalse();
    }

    #endregion
}
