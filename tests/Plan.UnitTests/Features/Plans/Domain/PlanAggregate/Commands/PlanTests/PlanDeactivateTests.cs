namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanDeactivateTests
{
    private readonly PlanValidator _validator = new();
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;
    private readonly Plan.Deactivate _deactivate;

    public PlanDeactivateTests()
    {
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _deactivate = new(_validator);
    }

    private TestablePlan CreateActivePlan()
    {
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", true));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(9.99m, Currency.EUR), true, [provider]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Active Plan");
        plan.SetDescription("Plan description");
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddPricingTier(tier);

        return plan;
    }

    [Fact]
    public void Execute_WithActivePlan_DeactivatesPlan()
    {
        var plan = CreateActivePlan();
        plan.IsActive.Should().BeTrue();

        var command = new DeactivatePlanCommand();

        var result = _deactivate.Execute(plan, command);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreateActivePlan();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalFeaturesCount = plan.Features.Count;
        var originalTiersCount = plan.PricingTiers.Count;

        var command = new DeactivatePlanCommand();

        var result = _deactivate.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Features.Should().HaveCount(originalFeaturesCount);
        result.PricingTiers.Should().HaveCount(originalTiersCount);
    }

    [Fact]
    public void Execute_WithInactivePlan_ThrowsConflictException()
    {
        var plan = CreateActivePlan();
        plan.SetIsActive(false);
        plan.IsActive.Should().BeFalse();

        var command = new DeactivatePlanCommand();

        var act = () => _deactivate.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*already inactive*");
    }

    [Fact]
    public void Execute_CannotDeactivateTwice()
    {
        var plan = CreateActivePlan();
        var command = new DeactivatePlanCommand();

        var result = _deactivate.Execute(plan, command);
        result.IsActive.Should().BeFalse();

        var act = () => _deactivate.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*already inactive*");
    }
}
