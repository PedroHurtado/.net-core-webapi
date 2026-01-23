namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanActivateTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.Activate _activate;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanActivateTests()
    {
        _activate = new(_validator);
        _createMoney = new(new MoneyValidator());
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
    }

    private TestablePlan CreateInactivePlan()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", true));

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Inactive Plan");
        plan.SetDescription("Plan description");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(false);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(provider);

        return plan;
    }

    [Fact]
    public void Execute_WithInactivePlan_ActivatesPlan()
    {
        var plan = CreateInactivePlan();
        plan.IsActive.Should().BeFalse();

        var command = new ActivatePlanCommand();

        var result = _activate.Execute(plan, command);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_PreservesOtherProperties()
    {
        var plan = CreateInactivePlan();
        var originalId = plan.Id;
        var originalName = plan.Name;
        var originalDescription = plan.Description;
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalFeaturesCount = plan.Features.Count;
        var originalProvidersCount = plan.ProviderConfigurations.Count;

        var command = new ActivatePlanCommand();

        var result = _activate.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.Features.Should().HaveCount(originalFeaturesCount);
        result.ProviderConfigurations.Should().HaveCount(originalProvidersCount);
    }

    [Fact]
    public void Execute_WithActivePlan_ThrowsConflictException()
    {
        var plan = CreateInactivePlan();
        plan.SetIsActive(true);
        plan.IsActive.Should().BeTrue();

        var command = new ActivatePlanCommand();

        var act = () => _activate.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public void Execute_CannotActivateTwice()
    {
        var plan = CreateInactivePlan();
        var command = new ActivatePlanCommand();

        // First activation should succeed
        var result = _activate.Execute(plan, command);
        result.IsActive.Should().BeTrue();

        // Second activation should throw
        var act = () => _activate.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*already active*");
    }
}
