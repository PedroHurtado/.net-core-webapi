namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanAddFeatureTests
{
    private readonly PlanValidator _validator = new();
    private readonly MoneyValidator _moneyValidator = new();
    private readonly FeatureValidator _featureValidator = new();
    private readonly Money.Create _createMoney;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;
    private readonly Plan.AddFeature _addFeature;

    public PlanAddFeatureTests()
    {
        _createMoney = new(_moneyValidator);
        _createFeature = new(_featureValidator);
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _addFeature = new(_createFeature, _validator);
    }

    private TestablePlan CreateValidPlan()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, Currency.EUR));
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
    public void Execute_WithLimitType_AddsFeature()
    {
        var plan = CreateValidPlan();
        var originalCount = plan.Features.Count;

        var command = new AddFeatureCommand(
            Code: "NEW_FEATURE",
            Name: "New Feature",
            Description: "Description",
            Type: FeatureType.Limit,
            Limit: 100,
            Unit: "units"
        );

        var result = _addFeature.Execute(plan, command);

        result.Features.Should().HaveCount(originalCount + 1);
        var addedFeature = result.Features.First(f => f.Code == "NEW_FEATURE");
        addedFeature.Name.Should().Be("New Feature");
        addedFeature.Description.Should().Be("Description");
        addedFeature.Type.Should().Be(FeatureType.Limit);
        addedFeature.Limit.Should().Be(100);
        addedFeature.Unit.Should().Be("units");
    }

    [Fact]
    public void Execute_WithBooleanType_AddsFeature()
    {
        var plan = CreateValidPlan();
        
        var command = new AddFeatureCommand(
            Code: "BOOL_FEATURE",
            Name: "Boolean Feature",
            Description: "",
            Type: FeatureType.Boolean,
            Limit: 0,
            Unit: ""
        );

        var result = _addFeature.Execute(plan, command);

        var addedFeature = result.Features.First(f => f.Code == "BOOL_FEATURE");
        addedFeature.Type.Should().Be(FeatureType.Boolean);
        addedFeature.Limit.Should().BeNull();
    }

    [Fact]
    public void Execute_WithUnlimitedType_AddsFeature()
    {
        var plan = CreateValidPlan();
        
        var command = new AddFeatureCommand(
            Code: "UNLIMITED_FEATURE",
            Name: "Unlimited Feature",
            Description: "",
            Type: FeatureType.Unlimited,
            Limit: 0,
            Unit: ""
        );

        var result = _addFeature.Execute(plan, command);

        var addedFeature = result.Features.First(f => f.Code == "UNLIMITED_FEATURE");
        addedFeature.Type.Should().Be(FeatureType.Unlimited);
        addedFeature.Limit.Should().BeNull();
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

        var command = new AddFeatureCommand(
            "TEST_FEATURE", "Test", "", FeatureType.Boolean, 0, ""
        );

        var result = _addFeature.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.IsActive.Should().Be(originalIsActive);
        result.ProviderConfigurations.Should().HaveCount(originalProvidersCount);
    }

    #region Conflict Throws (409)

    [Fact]
    public void Execute_WithDuplicateFeatureCode_ThrowsConflictException()
    {
        var plan = CreateValidPlan();
        var existingCode = plan.Features.First().Code;

        var command = new AddFeatureCommand(
            Code: existingCode,
            Name: "Duplicate",
            Description: "",
            Type: FeatureType.Boolean,
            Limit: 0,
            Unit: ""
        );

        var act = () => _addFeature.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage($"*code '{existingCode}' already exists*");
    }

    #endregion
}
