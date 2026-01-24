namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanDeactivateProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.DeactivateProviderConfiguration _deactivateProviderConfig;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanDeactivateProviderConfigurationTests()
    {
        _deactivateProviderConfig = new(_validator);
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
    }

    private TestablePlan CreateValidPlanWithMultipleProviders()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("FEATURE", "Feature", null, FeatureType.Boolean));
        var provider1 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_1", "price_1", true));
        var provider2 = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Paddle", "prod_2", "price_2", true));

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Original Plan");
        plan.SetDescription("Original description");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(provider1);
        plan.AddProviderConfiguration(provider2);

        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_DeactivatesConfiguration()
    {
        var plan = CreateValidPlanWithMultipleProviders();
        
        var command = new DeactivateProviderConfigurationCommand(Provider: "Stripe");

        var result = _deactivateProviderConfig.Execute(plan, command);

        var deactivatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Stripe");
        deactivatedConfig.IsActive.Should().BeFalse();
        
        // Paddle should remain active
        result.ProviderConfigurations.First(p => p.Provider == "Paddle").IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_DoesNothing()
    {
        var plan = CreateValidPlanWithMultipleProviders();
        // Deactivate first time
        _deactivateProviderConfig.Execute(plan, new DeactivateProviderConfigurationCommand("Stripe"));
        
        // Deactivate again
        var command = new DeactivateProviderConfigurationCommand("Stripe");
        var result = _deactivateProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.First(p => p.Provider == "Stripe").IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreateValidPlanWithMultipleProviders();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalIsActive = plan.IsActive;
        var originalFeaturesCount = plan.Features.Count;

        var command = new DeactivateProviderConfigurationCommand("Stripe");

        var result = _deactivateProviderConfig.Execute(plan, command);

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
    public void Execute_WhenDeactivatingLastActiveConfig_ThrowsValidationException()
    {
        var plan = CreateValidPlanWithMultipleProviders();
        // Deactivate one first so only one remains active
        _deactivateProviderConfig.Execute(plan, new DeactivateProviderConfigurationCommand("Stripe"));
        
        // Try to deactivate the last one (Paddle)
        var command = new DeactivateProviderConfigurationCommand("Paddle");

        var act = () => _deactivateProviderConfig.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one active provider*");
    }

    #endregion

    #region Not Found Throws (404)

    [Fact]
    public void Execute_WithNonExistentProvider_ThrowsKeyNotFoundException()
    {
        var plan = CreateValidPlanWithMultipleProviders();

        var command = new DeactivateProviderConfigurationCommand("NON_EXISTENT");

        var act = () => _deactivateProviderConfig.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion
}
