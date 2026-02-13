namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanActivateTests
{
    private readonly PlanValidator _validator = new();
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;
    private readonly Plan.Activate _activate;

    public PlanActivateTests()
    {
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _activate = new(_validator);
    }

    private TestablePlan CreateInactivePlan()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Inactive Plan");
        plan.SetDescription("Plan description");
        plan.SetIsActive(false);
        return plan;
    }

    private TestablePlan CreateReadyToActivatePlan()
    {
        var plan = CreateInactivePlan();
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", true));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);

        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

        return plan;
    }

    [Fact]
    public void Execute_WithReadyPlan_ActivatesPlan()
    {
        var plan = CreateReadyToActivatePlan();
        var command = new ActivatePlanCommand();

        var result = _activate.Execute(plan, command);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreateReadyToActivatePlan();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalFeaturesCount = plan.Features.Count;
        var originalTiersCount = plan.PricingTiers.Count;

        var command = new ActivatePlanCommand();

        var result = _activate.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Features.Should().HaveCount(originalFeaturesCount);
        result.PricingTiers.Should().HaveCount(originalTiersCount);
    }

    [Fact]
    public void Execute_WithActivePlan_ThrowsConflictException()
    {
        var plan = CreateReadyToActivatePlan();
        plan.SetIsActive(true);

        var command = new ActivatePlanCommand();

        var act = () => _activate.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public void Execute_WithNoFeatures_ThrowsValidationException()
    {
        var plan = CreateInactivePlan();
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", true));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);
        plan.AddPricingTier(tier);

        var command = new ActivatePlanCommand();

        var act = () => _activate.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one feature*");
    }

    [Fact]
    public void Execute_WithNoActivePricingTierWithProvider_ThrowsValidationException()
    {
        var plan = CreateInactivePlan();
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        plan.AddFeature(feature);

        var command = new ActivatePlanCommand();

        var act = () => _activate.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one active pricing tier*");
    }

    [Fact]
    public void Execute_WithInactiveProviderOnly_ThrowsValidationException()
    {
        var plan = CreateInactivePlan();
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", false));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);

        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

        var command = new ActivatePlanCommand();

        var act = () => _activate.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*at least one active pricing tier*");
    }
}
