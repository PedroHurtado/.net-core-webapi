namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanActivateProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.Create _createPlan;
    private readonly PlanAgg.AddFeature _addFeature;
    private readonly PlanAgg.AddProviderConfiguration _addProviderConfig;
    private readonly PlanAgg.ActivateProviderConfiguration _activateProviderConfig;
    private readonly PlanAgg.Activate _activate;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanActivateProviderConfigurationTests()
    {
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _createPlan = new(_createMoney, _validator);
        _addFeature = new(_createFeature, _validator);
        _addProviderConfig = new(_createProviderConfig, _validator);
        _activateProviderConfig = new(_createProviderConfig, _validator);
        _activate = new(_validator);
    }

    private PlanAgg CreatePlanWithInactiveProvider()
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
            "Paddle", "prod_2", "price_2", false));

        plan = _activate.Execute(plan, new ActivatePlanCommand());

        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_ActivatesConfiguration()
    {
        var plan = CreatePlanWithInactiveProvider();

        var command = new ActivateProviderConfigurationCommand(Provider: "Paddle");

        var result = _activateProviderConfig.Execute(plan, command);

        var activatedConfig = result.ProviderConfigurations.First(p => p.Provider == "Paddle");
        activatedConfig.IsActive.Should().BeTrue();

        result.ProviderConfigurations.First(p => p.Provider == "Stripe").IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ReturnsWithoutChanges()
    {
        var plan = CreatePlanWithInactiveProvider();

        var command = new ActivateProviderConfigurationCommand("Stripe");

        var result = _activateProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.First(p => p.Provider == "Stripe").IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreatePlanWithInactiveProvider();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalIsActive = plan.IsActive;
        var originalFeaturesCount = plan.Features.Count;

        var command = new ActivateProviderConfigurationCommand("Paddle");

        var result = _activateProviderConfig.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.IsActive.Should().Be(originalIsActive);
        result.Features.Should().HaveCount(originalFeaturesCount);
    }

    #region Not Found Throws (404)

    [Fact]
    public void Execute_WithNonExistentProvider_ThrowsKeyNotFoundException()
    {
        var plan = CreatePlanWithInactiveProvider();

        var command = new ActivateProviderConfigurationCommand("NON_EXISTENT");

        var act = () => _activateProviderConfig.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Configuration for 'NON_EXISTENT' not found*");
    }

    #endregion
}
