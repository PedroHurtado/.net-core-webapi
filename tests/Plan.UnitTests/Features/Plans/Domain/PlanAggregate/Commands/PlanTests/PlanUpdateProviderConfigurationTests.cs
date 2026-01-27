namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanUpdateProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddFeature _addFeature;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;
    private readonly Plan.UpdateProviderConfiguration _updateProviderConfig;
    private readonly Plan.Activate _activate;
    private readonly Money.Create _createMoney;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;

    public PlanUpdateProviderConfigurationTests()
    {
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _createPlan = new(_createMoney, _validator);
        _addFeature = new(_createFeature, _validator);
        _addProviderConfig = new(_createProviderConfig, _validator);
        _updateProviderConfig = new(_createProviderConfig, _validator);
        _activate = new(_validator);
    }

    private Plan CreatePlanWithProviders()
    {
        var plan = _createPlan.Execute(new CreatePlanCommand(
            "Test Plan",
            "Test description",
            10m,
            "EUR",
            BillingPeriod.Monthly));

        plan = _addFeature.Execute(plan, new AddFeatureCommand(
            "TEST_FEATURE", "Test Feature", null, FeatureType.Boolean));

        plan = _addProviderConfig.Execute(plan, new AddProviderConfigurationCommand(
            "Stripe", "prod_stripe", "price_stripe", true));

        plan = _addProviderConfig.Execute(plan, new AddProviderConfigurationCommand(
            "Paddle", "prod_paddle", "price_paddle", false));

        plan = _activate.Execute(plan, new ActivatePlanCommand());

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
            ExternalPriceId: "price_stripe_updated");

        var result = _updateProviderConfig.Execute(plan, command);

        var updatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Stripe");
        updatedConfig.ExternalProductId.Should().Be("prod_stripe_updated");
        updatedConfig.ExternalPriceId.Should().Be("price_stripe_updated");
        updatedConfig.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesIsActiveStatus()
    {
        var plan = CreatePlanWithProviders();
        var paddleConfig = plan.ProviderConfigurations.First(p => p.Provider == "Paddle");
        paddleConfig.IsActive.Should().BeFalse();

        var command = new UpdateProviderConfigurationCommand(
            "Paddle",
            "prod_paddle_new",
            "price_paddle_new");

        var result = _updateProviderConfig.Execute(plan, command);

        var updatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Paddle");
        updatedConfig.ExternalProductId.Should().Be("prod_paddle_new");
        updatedConfig.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesOtherProviderConfigurations()
    {
        var plan = CreatePlanWithProviders();
        var originalPaddleConfig = plan.ProviderConfigurations.First(p => p.Provider == "Paddle");

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            "prod_stripe_updated",
            "price_stripe_updated");

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
            "price_new");

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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyExternalProductId_ThrowsValidationException(string? productId)
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "Stripe",
            productId!,
            "price_stripe");

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
            priceId!);

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
            "price_stripe");

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
            new string('a', 101));

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion

    #region Not Found Throws (404)

    [Fact]
    public void Execute_WithNonExistentProvider_ThrowsKeyNotFoundException()
    {
        var plan = CreatePlanWithProviders();

        var command = new UpdateProviderConfigurationCommand(
            "PayPal",
            "prod_paypal",
            "price_paypal");

        var act = () => _updateProviderConfig.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Configuration for 'PayPal' not found*");
    }

    #endregion
}
