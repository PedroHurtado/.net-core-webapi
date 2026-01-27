namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanUpdateFeatureTests
{
    private readonly PlanValidator _validator = new();
    private readonly FeatureValidator _featureValidator = new();
    private readonly MoneyValidator _moneyValidator = new();
    private readonly Money.Create _createMoney;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;
    private readonly Plan.UpdateFeature _updateFeature;

    public PlanUpdateFeatureTests()
    {
        _createMoney = new(_moneyValidator);
        _createFeature = new(_featureValidator);
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _updateFeature = new(_createFeature, _validator);
    }

    private TestablePlan CreatePlanWithFeature(string code, FeatureType type, int? limit = null)
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, Currency.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand(code, "Original Name", "Original Desc", type, limit, "units"));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", true));

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Test Plan");
        plan.SetDescription("Description");
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
        var plan = CreatePlanWithFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);
        var originalCount = plan.Features.Count;

        var command = new UpdateFeatureCommand(
            Code: "RESERVATIONS_MONTHLY",
            Name: "Reservas",
            Description: "Updated Desc",
            Type: FeatureType.Limit,
            Limit: 200,
            Unit: "bookings"
        );

        var result = _updateFeature.Execute(plan, command);

        result.Features.Should().HaveCount(originalCount);
        var updatedFeature = result.Features.First(f => f.Code == "RESERVATIONS_MONTHLY");
        updatedFeature.Name.Should().Be("Reservas");
        updatedFeature.Description.Should().Be("Updated Desc");
        updatedFeature.Type.Should().Be(FeatureType.Limit);
        updatedFeature.Limit.Should().Be(200);
        updatedFeature.Unit.Should().Be("bookings");
    }

    [Fact]
    public void Execute_ChangeTypeToUnlimited_UpdatesFeature()
    {
        var plan = CreatePlanWithFeature("RESERVATIONS_MONTHLY", FeatureType.Limit, 100);
        
        var command = new UpdateFeatureCommand(
            Code: "RESERVATIONS_MONTHLY",
            Name: "Reservas",
            Description: "Updated Desc",
            Type: FeatureType.Unlimited,
            Limit: 0, // Should be ignored/nullified
            Unit: "bookings"
        );

        var result = _updateFeature.Execute(plan, command);

        var updatedFeature = result.Features.First(f => f.Code == "RESERVATIONS_MONTHLY");
        updatedFeature.Type.Should().Be(FeatureType.Unlimited);
        updatedFeature.Limit.Should().BeNull();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreatePlanWithFeature("TEST_FEATURE", FeatureType.Boolean);
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalIsActive = plan.IsActive;

        var command = new UpdateFeatureCommand(
            Code: "TEST_FEATURE",
            Name: "Updated",
            Description: "",
            Type: FeatureType.Boolean,
            Limit: 0,
            Unit: ""
        );

        var result = _updateFeature.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.IsActive.Should().Be(originalIsActive);
    }

    #region NotFound Throws (404)

    [Fact]
    public void Execute_WithNonExistentFeature_ThrowsNotFoundException()
    {
        var plan = CreatePlanWithFeature("EXISTING_FEATURE", FeatureType.Boolean);

        var command = new UpdateFeatureCommand(
            Code: "NONEXISTENT",
            Name: "New Feature",
            Description: "",
            Type: FeatureType.Boolean,
            Limit: 0,
            Unit: ""
        );

        var act = () => _updateFeature.Execute(plan, command);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*code 'NONEXISTENT' not found*");
    }

    #endregion
}
