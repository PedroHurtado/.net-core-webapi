namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanUpdateFeatureTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.UpdateFeature _updateFeature;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanUpdateFeatureTests()
    {
        _updateFeature = new(_validator);
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
    public void Execute_WithValidCommand_UpdatesFeature()
    {
        var plan = CreateValidPlan();
        var originalCount = plan.Features.Count;

        var command = new UpdateFeatureCommand(
            Code: "ORIGINAL_FEATURE",
            Name: "Updated Feature",
            Description: "Updated Description",
            Type: FeatureType.Limit,
            Limit: 50,
            Unit: "users"
        );

        var result = _updateFeature.Execute(plan, command);

        result.Features.Should().HaveCount(originalCount);
        var updatedFeature = result.Features.First(f => f.Code == "ORIGINAL_FEATURE");
        updatedFeature.Name.Should().Be("Updated Feature");
        updatedFeature.Description.Should().Be("Updated Description");
        updatedFeature.Type.Should().Be(FeatureType.Limit);
        updatedFeature.Limit.Should().Be(50);
        updatedFeature.Unit.Should().Be("users");
    }

    [Fact]
    public void Execute_WithBooleanType_IgnoresLimit()
    {
        var plan = CreateValidPlan();
        
        var command = new UpdateFeatureCommand(
            Code: "ORIGINAL_FEATURE",
            Name: "Updated Boolean",
            Description: "",
            Type: FeatureType.Boolean,
            Limit: 999, // Should be ignored
            Unit: ""
        );

        var result = _updateFeature.Execute(plan, command);

        var updatedFeature = result.Features.First(f => f.Code == "ORIGINAL_FEATURE");
        updatedFeature.Type.Should().Be(FeatureType.Boolean);
        updatedFeature.Limit.Should().BeNull();
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
        var originalProvidersCount = plan.ProviderConfigurations.Count;

        var command = new UpdateFeatureCommand(
            "ORIGINAL_FEATURE", "Updated", "", FeatureType.Boolean, 0, ""
        );

        var result = _updateFeature.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.IsActive.Should().Be(originalIsActive);
        result.ProviderConfigurations.Should().HaveCount(originalProvidersCount);
    }

    #region Validation Throws (422)

    [Fact]
    public void Execute_WithInvalidLimitForLimitType_ThrowsValidationException()
    {
        var plan = CreateValidPlan();

        var command = new UpdateFeatureCommand(
            Code: "ORIGINAL_FEATURE",
            Name: "Invalid Limit",
            Description: "",
            Type: FeatureType.Limit,
            Limit: 0, // Invalid: must be > 0
            Unit: "units"
        );

        var act = () => _updateFeature.Execute(plan, command);

        act.Should().Throw<ValidationException>()
            .WithMessage("*limit value greater than 0*");
    }

    #endregion

    #region Not Found Throws (404)

    [Fact]
    public void Execute_WithNonExistentFeature_ThrowsKeyNotFoundException()
    {
        var plan = CreateValidPlan();

        var command = new UpdateFeatureCommand(
            Code: "NON_EXISTENT",
            Name: "New Feature",
            Description: "",
            Type: FeatureType.Boolean,
            Limit: 0,
            Unit: ""
        );

        var act = () => _updateFeature.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion
}
