namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanDeactivateTests
{
    private readonly PlanValidator _validator = new();
    private readonly MoneyValidator _moneyValidator = new();
    private readonly Money.Create _createMoney;
    private readonly Feature.Create _createFeature;
    private readonly PaymentProviderConfig.Create _createProviderConfig;
    private readonly Plan.Deactivate _deactivate;

    public PlanDeactivateTests()
    {
        _createMoney = new(_moneyValidator);
        _createFeature = new(new FeatureValidator());
        _createProviderConfig = new(new PaymentProviderConfigValidator());
        _deactivate = new(_validator);
    }

    private TestablePlan CreateActivePlan()
    {
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, Currency.EUR));
        var feature = _createFeature.Execute(new CreateFeatureCommand("TEST_FEATURE", "Test", null, FeatureType.Boolean));
        var provider = _createProviderConfig.Execute(new CreatePaymentProviderConfigCommand("Stripe", "prod_test", "price_test", true));

        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Active Plan");
        plan.SetDescription("Plan description");
        plan.SetPrice(price);
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        plan.AddFeature(feature);
        plan.AddProviderConfiguration(provider);

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
        var originalPrice = plan.Price;
        var originalBillingPeriod = plan.BillingPeriod;
        var originalFeaturesCount = plan.Features.Count;
        var originalProvidersCount = plan.ProviderConfigurations.Count;

        var command = new DeactivatePlanCommand();

        var result = _deactivate.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.Name.Should().Be(originalName);
        result.Description.Should().Be(originalDescription);
        result.Price.Should().Be(originalPrice);
        result.BillingPeriod.Should().Be(originalBillingPeriod);
        result.Features.Should().HaveCount(originalFeaturesCount);
        result.ProviderConfigurations.Should().HaveCount(originalProvidersCount);
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

        // First deactivation should succeed
        var result = _deactivate.Execute(plan, command);
        result.IsActive.Should().BeFalse();

        // Second deactivation should throw
        var act = () => _deactivate.Execute(plan, command);

        act.Should().Throw<ConflictException>()
            .WithMessage("*already inactive*");
    }
}
