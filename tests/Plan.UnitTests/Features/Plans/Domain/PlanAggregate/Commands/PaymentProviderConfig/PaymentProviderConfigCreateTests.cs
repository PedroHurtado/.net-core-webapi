namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PaymentProviderConfig;

public class PaymentProviderConfigCreateTests
{
    private readonly PaymentProviderConfigValidator _validator = new();
    private readonly PaymentProviderConfigVO.Create _create;

    public PaymentProviderConfigCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsPaymentProviderConfig()
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "price_456");

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Provider.Should().Be("Stripe");
        result.ExternalProductId.Should().Be("prod_123");
        result.ExternalPriceId.Should().Be("price_456");
        result.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("Stripe")]
    [InlineData("Paddle")]
    [InlineData("PayPal")]
    public void Execute_WithDifferentProviders_SetsCorrectProvider(string provider)
    {
        var command = new CreatePaymentProviderConfigCommand(provider, "prod_123", "price_456");

        var result = _create.Execute(command);

        result.Provider.Should().Be(provider);
    }

    [Theory]
    [InlineData("prod_123")]
    [InlineData("product_abc")]
    [InlineData("ext_prod_xyz")]
    public void Execute_WithDifferentProductIds_SetsCorrectProductId(string productId)
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", productId, "price_456");

        var result = _create.Execute(command);

        result.ExternalProductId.Should().Be(productId);
    }

    [Theory]
    [InlineData("price_123")]
    [InlineData("price_abc")]
    [InlineData("ext_price_xyz")]
    public void Execute_WithDifferentPriceIds_SetsCorrectPriceId(string priceId)
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", "prod_123", priceId);

        var result = _create.Execute(command);

        result.ExternalPriceId.Should().Be(priceId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Execute_WithIsActive_SetsCorrectValue(bool isActive)
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "price_456", isActive);

        var result = _create.Execute(command);

        result.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void Execute_WithDefaultIsActive_SetsToTrue()
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "price_456");

        var result = _create.Execute(command);

        result.IsActive.Should().BeTrue();
    }

    #region Validation Throws

    [Fact]
    public void Execute_WithEmptyProvider_ThrowsValidationException()
    {
        var command = new CreatePaymentProviderConfigCommand("", "prod_123", "price_456");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithProviderTooLong_ThrowsValidationException()
    {
        var longProvider = new string('a', 51);
        var command = new CreatePaymentProviderConfigCommand(longProvider, "prod_123", "price_456");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEmptyExternalProductId_ThrowsValidationException()
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", "", "price_456");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithExternalProductIdTooLong_ThrowsValidationException()
    {
        var longProductId = new string('a', 101);
        var command = new CreatePaymentProviderConfigCommand("Stripe", longProductId, "price_456");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithEmptyExternalPriceId_ThrowsValidationException()
    {
        var command = new CreatePaymentProviderConfigCommand("Stripe", "prod_123", "");

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithExternalPriceIdTooLong_ThrowsValidationException()
    {
        var longPriceId = new string('a', 101);
        var command = new CreatePaymentProviderConfigCommand("Stripe", "prod_123", longPriceId);

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
