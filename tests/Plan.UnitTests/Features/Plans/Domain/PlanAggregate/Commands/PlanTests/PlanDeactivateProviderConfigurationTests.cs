namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanDeactivateProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddFeature _addFeature;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;
    private readonly Plan.DeactivateProviderConfiguration _deactivateProviderConfig;
    private readonly Plan.Activate _activate;
    private readonly Money.Create _createMoney;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;

    public PlanDeactivateProviderConfigurationTests()
    {
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _createPlan = new(_createMoney, _validator);
        _addFeature = new(_createFeature, _validator);
        _addProviderConfig = new(_createProviderConfig, _validator);
        _deactivateProviderConfig = new(_createProviderConfig, _validator);
        _activate = new(_validator);
    }

    private Plan CreatePlanWithMultipleActiveProviders()
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
            "Stripe", "prod_1", "price_1", true));

        plan = _addProviderConfig.Execute(plan, new AddProviderConfigurationCommand(
            "Paddle", "prod_2", "price_2", true));

        plan = _activate.Execute(plan, new ActivatePlanCommand());

        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_DeactivatesConfiguration()
    {
        var plan = CreatePlanWithMultipleActiveProviders();

        var command = new DeactivateProviderConfigurationCommand(Provider: "Stripe");

        var result = _deactivateProviderConfig.Execute(plan, command);

        var deactivatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Stripe");
        deactivatedConfig.IsActive.Should().BeFalse();

        result.ProviderConfigurations.First(p => p.Provider == "Paddle").IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ReturnsWithoutChanges()
    {
        var plan = CreatePlanWithMultipleActiveProviders();
        plan = _deactivateProviderConfig.Execute(plan, new DeactivateProviderConfigurationCommand("Stripe"));

        var command = new DeactivateProviderConfigurationCommand("Stripe");

        var result = _deactivateProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.First(p => p.Provider == "Stripe").IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreatePlanWithMultipleActiveProviders();
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
    public void Execute_WhenDeactivatingLastActiveConfigOnActivePlan_ThrowsValidationException()
    {
        var plan = CreatePlanWithMultipleActiveProviders();
        plan = _deactivateProviderConfig.Execute(plan, new DeactivateProviderConfigurationCommand("Stripe"));

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
        var plan = CreatePlanWithMultipleActiveProviders();

        var command = new DeactivateProviderConfigurationCommand("NON_EXISTENT");

        var act = () => _deactivateProviderConfig.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Configuration for 'NON_EXISTENT' not found*");
    }

    #endregion
}
