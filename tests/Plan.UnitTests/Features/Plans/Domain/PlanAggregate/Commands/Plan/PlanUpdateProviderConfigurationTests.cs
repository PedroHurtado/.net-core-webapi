namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanUpdateProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.UpdateProviderConfiguration _updateProviderConfig;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanUpdateProviderConfigurationTests()
    {
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _updateProviderConfig = new(_validator, _createProviderConfig);
    }

    private TestablePlan CreatePlanWithProviders()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        var provider1 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_stripe", "price_stripe", true));
        var provider2 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Paddle", "prod_paddle", "price_paddle", false));

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Test Plan");
        plan.SetDescription("Plan description");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(provider1);
        plan.AddProviderConfiguration(provider2);

        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_UpdatesProviderConfiguration()
    {
        var plan = CreatePlanWithProviders();
        var originalStripeConfig = plan.ProviderConfigurations.First(p => p.Provider == "Stripe");
        originalStripeConfig.ExternalProductId.Should().Be("prod_stripe");
        originalStripeConfig.ExternalPriceId.Should().Be("price_stripe");
        originalStripeConfig.IsActive.Should().BeTrue();

        var command = new UpdateProviderConfigurationCommand(
            Provider: "Stripe",
            ExternalProductId: "prod_stripe_updated",
            ExternalPriceId: "price_stripe_updated",
            IsActive: true
        );

        var result = _updateProviderConfig.Execute(plan, command);

        var updatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Stripe");
        updatedConfig.ExternalProductId.Should().Be("prod_stripe_updated");
        updatedConfig.ExternalPriceId.Should().Be("price_stripe_updated");
        updatedConfig.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_CanChangeProviderActiveStatus()
    {
        var plan = CreatePlanWithProviders();
        var paddleConfig = plan.ProviderConfigurations.First(p => p.Provider == "Paddle");
        paddleConfig.IsActive.Should().BeFalse();

        var command = new UpdateProviderConfigurationCommand(
            "Paddle",
            "prod_paddle_new",
            "price_paddle_new",
            true
        );

        var result = _updateProviderConfig.Execute(plan, command);

        var updatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Paddle");
        updatedConfig.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesOtherProviderConfigurations()
    {
        var plan = CreatePlanWithProviders();
        var originalPaddleConfig = plan.ProviderConfigurations.First(p => p.Provider == "Paddle");

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            "prod_stripe_updated",
            "price_stripe_updated",
            true
        );

        var result = _updateProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.Should().HaveCount(2);
        var paddleConfig = result.ProviderConfigurations.First(p => p.Provider == "Paddle");
        paddleConfig.ExternalProductId.Should().Be(originalPaddleConfig.ExternalProductId);
        paddleConfig.ExternalPriceId.Should().Be(originalPaddleConfig.ExternalPriceId);
        paddleConfig.IsActive.Should().Be(originalPaddleConfig.IsActive);
    }

    [Fact]
    public void Execute_PreservesOtherPlanProperties()
    {
        var plan = CreatePlanWithProviders();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalIsActive = plan.IsActive;
        var originalFeaturesCount = plan.Features.Count;

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            "prod_new",
            "price_new",
            true
        );

        var result = _updateProviderConfig.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.IsActive.Should().Be(originalIsActive);
        result.Features.Should().HaveCount(originalFeaturesCount);
    }

    #region Validation Throws (422)

    [Fact]
    public void Execute_WithNonExistentProvider_ThrowsValidationException()
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "PayPal",
            "prod_paypal",
            "price_paypal",
            true
        );

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void Execute_DeactivatingLastActiveProvider_ThrowsValidationException()
    {
        var plan = CreatePlanWithProviders();
        // Stripe is the only active provider

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            "prod_stripe",
            "price_stripe",
            false // Trying to deactivate
        );

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one active provider*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyExternalProductId_ThrowsValidationException(string? productId)
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            productId!,
            "price_stripe",
            true
        );

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyExternalPriceId_ThrowsValidationException(string? priceId)
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            "prod_stripe",
            priceId!,
            true
        );

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithExternalProductIdExceedingMaxLength_ThrowsValidationException()
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            new string('a', 101),
            "price_stripe",
            true
        );

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithExternalPriceIdExceedingMaxLength_ThrowsValidationException()
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            "prod_stripe",
            new string('a', 101),
            true
        );

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
