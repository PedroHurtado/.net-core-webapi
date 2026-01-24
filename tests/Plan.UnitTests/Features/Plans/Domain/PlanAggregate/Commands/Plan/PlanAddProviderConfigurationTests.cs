namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanAddProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.AddProviderConfiguration _addProviderConfig;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanAddProviderConfigurationTests()
    {
        _addProviderConfig = new(_validator);
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
    }

    private TestablePlan CreateValidPlan()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("ORIGINAL_FEATURE", "Original", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_original", "price_original", true));

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Original Plan");
        plan.SetDescription("Original description");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(provider);

        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_AddsProviderConfiguration()
    {
        var plan = CreateValidPlan();
        var originalCount = plan.ProviderConfigurations.Count;

        var command = new AddProviderConfigurationCommand(
            Provider: "Paddle",
            ExternalProductId: "prod_paddle",
            ExternalPriceId: "price_paddle",
            IsActive: true
        );

        var result = _addProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.Should().HaveCount(originalCount + 1);
        var addedConfig = result.ProviderConfigurations.First(p => p.Provider == "Paddle");
        addedConfig.ExternalProductId.Should().Be("prod_paddle");
        addedConfig.ExternalPriceId.Should().Be("price_paddle");
        addedConfig.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithInactiveConfiguration_AddsProviderConfiguration()
    {
        var plan = CreateValidPlan();
        var originalCount = plan.ProviderConfigurations.Count;

        var command = new AddProviderConfigurationCommand(
            Provider: "Paddle",
            ExternalProductId: "prod_paddle",
            ExternalPriceId: "price_paddle",
            IsActive: false
        );

        var result = _addProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.Should().HaveCount(originalCount + 1);
        var addedConfig = result.ProviderConfigurations.First(p => p.Provider == "Paddle");
        addedConfig.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreateValidPlan();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalIsActive = plan.IsActive;
        var originalFeaturesCount = plan.Features.Count;

        var command = new AddProviderConfigurationCommand(
            "Paddle", "prod", "price", true
        );

        var result = _addProviderConfig.Execute(plan, command);

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
    public void Execute_WithEmptyProvider_ThrowsValidationException(string? provider)
    {
        var plan = CreateValidPlan();

        var command = new AddProviderConfigurationCommand(
            Provider: provider!,
            ExternalProductId: "prod",
            ExternalPriceId: "price",
            IsActive: true
        );

        var act = () => _addProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*Provider*");
    }

    #endregion

    #region Conflict Throws (409)

    [Fact]
    public void Execute_WithDuplicateActiveProvider_ThrowsConflictException()
    {
        var plan = CreateValidPlan();
        // Plan already has active Stripe config

        var command = new AddProviderConfigurationCommand(
            Provider: "Stripe",
            ExternalProductId: "prod_new",
            ExternalPriceId: "price_new",
            IsActive: true
        );

        var act = () => _addProviderConfig.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*active configuration for provider 'Stripe' already exists*");
    }

    [Fact]
    public void Execute_WithDuplicateProviderButInactive_Succeeds()
    {
        var plan = CreateValidPlan();
        // Plan already has active Stripe config

        var command = new AddProviderConfigurationCommand(
            Provider: "Stripe",
            ExternalProductId: "prod_new",
            ExternalPriceId: "price_new",
            IsActive: false
        );

        var result = _addProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.Count(p => p.Provider == "Stripe").Should().Be(2);
        result.ProviderConfigurations.Count(p => p.Provider == "Stripe" && p.IsActive).Should().Be(1);
    }

    #endregion
}
