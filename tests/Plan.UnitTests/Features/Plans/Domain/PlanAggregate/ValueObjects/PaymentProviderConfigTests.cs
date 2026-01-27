namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.ValueObjects;

public class PaymentProviderConfigTests
{
    [Theory]
    [InlineData("Stripe")]
    [InlineData("Paddle")]
    [InlineData("PayPal")]
    public void Provider_SetsCorrectValue(string provider)
    {
        var config = new TestablePaymentProviderConfig(provider, "prod_123", "price_456");

        config.Provider.Should().Be(provider);
    }

    [Theory]
    [InlineData("prod_123")]
    [InlineData("product_abc")]
    [InlineData("ext_prod_xyz")]
    public void ExternalProductId_SetsCorrectValue(string externalProductId)
    {
        var config = new TestablePaymentProviderConfig("Stripe", externalProductId, "price_456");

        config.ExternalProductId.Should().Be(externalProductId);
    }

    [Theory]
    [InlineData("price_123")]
    [InlineData("price_abc")]
    [InlineData("ext_price_xyz")]
    public void ExternalPriceId_SetsCorrectValue(string externalPriceId)
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", externalPriceId);

        config.ExternalPriceId.Should().Be(externalPriceId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsActive_SetsCorrectValue(bool isActive)
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456", isActive);

        config.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void IsActive_DefaultsToTrue()
    {
        var config = new TestablePaymentProviderConfig("Stripe", "prod_123", "price_456");

        config.IsActive.Should().BeTrue();
    }
}
