namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanAddProviderConfigurationTests
{
    private readonly PlanValidator _validator = new();
    private readonly Plan.Create _createPlan;
    private readonly Plan.AddFeature _addFeature;
    private readonly Plan.AddProviderConfiguration _addProviderConfig;
    private readonly Plan.Activate _activate;
    private readonly Money.Create _createMoney;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;

    public PlanAddProviderConfigurationTests()
    {
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _createPlan = new(_createMoney, _validator);
        _addFeature = new(_createFeature, _validator);
        _addProviderConfig = new(_createProviderConfig, _validator);
        _activate = new(_validator);
    }

    private Plan CreateValidPlan()
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
            "Stripe", "prod_original", "price_original", true));

        plan = _activate.Execute(plan, new ActivatePlanCommand());

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
            IsActive: true);

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
            IsActive: false);

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
            "Paddle", "prod", "price", true);

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
            IsActive: true);

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

        var command = new AddProviderConfigurationCommand(
            Provider: "Stripe",
            ExternalProductId: "prod_new",
            ExternalPriceId: "price_new",
            IsActive: true);

        var act = () => _addProviderConfig.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Active configuration for 'Stripe' already exists*");
    }

    [Fact]
    public void Execute_WithDuplicateProviderButInactive_Succeeds()
    {
        var plan = CreateValidPlan();

        var command = new AddProviderConfigurationCommand(
            Provider: "Stripe",
            ExternalProductId: "prod_new",
            ExternalPriceId: "price_new",
            IsActive: false);

        var result = _addProviderConfig.Execute(plan, command);

        result.ProviderConfigurations.Count(p => p.Provider == "Stripe").Should().Be(2);
        result.ProviderConfigurations.Count(p => p.Provider == "Stripe" && p.IsActive).Should().Be(1);
    }

    #endregion
}
