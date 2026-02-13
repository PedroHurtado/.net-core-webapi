namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanRemoveFeatureTests
{
    private readonly PlanValidator _validator = new();
    private readonly Plan.RemoveFeature _removeFeature;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;

    public PlanRemoveFeatureTests()
    {
        _removeFeature = new(_validator);
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
    }

    private TestablePlan CreateValidPlanWithMultipleFeatures()
    {
        var feature1 = _createFeature.Execute(new CreateFeatureCommand("FEATURE_1", "Feature 1", null, FeatureType.Boolean));
        var feature2 = _createFeature.Execute(new CreateFeatureCommand("FEATURE_2", "Feature 2", null, FeatureType.Limit, 10, "units"));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_original", "price_original", true));
        var tier = new PricingTier(BillingPeriod.Monthly, new TestableMoney(10m, Currency.EUR), true, [provider]);

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Original Plan");
        plan.SetDescription("Original description");
        plan.SetIsActive(true);
        plan.AddFeature(feature1);
        plan.AddFeature(feature2);
        plan.AddPricingTier(tier);

        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_RemovesFeature()
    {
        var plan = CreateValidPlanWithMultipleFeatures();
        var originalCount = plan.Features.Count; // Should be 2

        var command = new RemoveFeatureCommand(Code: "FEATURE_1");

        var result = _removeFeature.Execute(plan, command);

        result.Features.Should().HaveCount(originalCount - 1);
        result.Features.Should().NotContain(f => f.Code == "FEATURE_1");
        result.Features.Should().Contain(f => f.Code == "FEATURE_2");
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreateValidPlanWithMultipleFeatures();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalIsActive = plan.IsActive;
        var originalTiersCount = plan.PricingTiers.Count;

        var command = new RemoveFeatureCommand("FEATURE_1");

        var result = _removeFeature.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.IsActive.Should().Be(originalIsActive);
        result.PricingTiers.Should().HaveCount(originalTiersCount);
    }

    #region Validation Throws (422)

    [Fact]
    public void Execute_WhenRemovingLastFeature_ThrowsValidationException()
    {
        var plan = CreateValidPlanWithMultipleFeatures();
        // Remove one first so only one remains
        _removeFeature.Execute(plan, new RemoveFeatureCommand("FEATURE_1"));

        plan.Features.Should().HaveCount(1);
        var lastFeatureCode = plan.Features.First().Code;

        var command = new RemoveFeatureCommand(lastFeatureCode);

        var act = () => _removeFeature.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*Cannot remove last feature from active plan*");
    }

    #endregion

    #region Not Found Throws (404)

    [Fact]
    public void Execute_WithNonExistentFeature_ThrowsKeyNotFoundException()
    {
        var plan = CreateValidPlanWithMultipleFeatures();

        var command = new RemoveFeatureCommand("NON_EXISTENT");

        var act = () => _removeFeature.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion
}
