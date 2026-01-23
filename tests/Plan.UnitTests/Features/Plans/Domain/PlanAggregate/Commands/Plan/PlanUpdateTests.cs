namespace Plan.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.Plan;

public class PlanUpdateTests
{
    private readonly PlanValidator _validator = new();
    private readonly PlanAgg.Update _update;
    private readonly MoneyVO.Create _createMoney;
    private readonly FeatureVO.Create _createFeature;
    private readonly PaymentProviderConfigVO.Create _createProviderConfig;

    public PlanUpdateTests()
    {
        _update = new(_validator);
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
    public void Execute_WithValidCommand_UpdatesPlan()
    {
        var plan = CreateValidPlan();
        var newPrice = _createMoney.Execute(new CreateMoneyCommand(20m, CurrencyVO.USD));

        var command = new UpdatePlanCommand(
            Name: "Updated Plan",
            Description: "Updated description",
            Price: newPrice,
            BillingPeriod: BillingPeriod.Yearly
        );

        var result = _update.Execute(plan, command);

        result.Name.Should().Be("Updated Plan");
        result.Description.Should().Be("Updated description");
        result.Price.Amount.Should().Be(20m);
        result.Price.Currency.Should().Be(CurrencyVO.USD);
        result.BillingPeriod.Should().Be(BillingPeriod.Yearly);
    }

    [Fact]
    public void Execute_WithDifferentBillingPeriod_UpdatesBillingPeriod()
    {
        var plan = CreateValidPlan();
        plan.BillingPeriod.Should().Be(BillingPeriod.Monthly);

        var price = _createMoney.Execute(new CreateMoneyCommand(15m, CurrencyVO.GBP));
        var command = new UpdatePlanCommand(
            "Plan Name",
            "Description",
            price,
            BillingPeriod.Quarterly
        );

        var result = _update.Execute(plan, command);

        result.BillingPeriod.Should().Be(BillingPeriod.Quarterly);
    }

    [Fact]
    public void Execute_PreservesNonUpdatedProperties()
    {
        var plan = CreateValidPlan();
        var originalId = plan.Id;
        var originalIsActive = plan.IsActive;
        var originalFeaturesCount = plan.Features.Count;
        var originalProvidersCount = plan.ProviderConfigurations.Count;

        var price = _createMoney.Execute(new CreateMoneyCommand(15m, CurrencyVO.EUR));
        var command = new UpdatePlanCommand(
            "New Name",
            "New description",
            price,
            BillingPeriod.Monthly
        );

        var result = _update.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.IsActive.Should().Be(originalIsActive);
        result.Features.Should().HaveCount(originalFeaturesCount);
        result.ProviderConfigurations.Should().HaveCount(originalProvidersCount);
    }

    [Fact]
    public void Execute_DoesNotModifyCollections()
    {
        var plan = CreateValidPlan();
        var originalFeature = plan.Features.First();
        var originalProvider = plan.ProviderConfigurations.First();

        var price = _createMoney.Execute(new CreateMoneyCommand(25m, CurrencyVO.USD));
        var command = new UpdatePlanCommand(
            "Updated Plan",
            "Updated description",
            price,
            BillingPeriod.Yearly
        );

        var result = _update.Execute(plan, command);

        result.Features.Should().HaveCount(1);
        result.Features.First().Code.Should().Be(originalFeature.Code);
        result.ProviderConfigurations.Should().HaveCount(1);
        result.ProviderConfigurations.First().Provider.Should().Be(originalProvider.Provider);
    }

    #region Validation Throws - Basic Properties

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var plan = CreateValidPlan();
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));

        var command = new UpdatePlanCommand(
            name!,
            "Description",
            price,
            BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var plan = CreateValidPlan();
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));

        var command = new UpdatePlanCommand(
            new string('a', 101),
            "Description",
            price,
            BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyDescription_ThrowsValidationException(string? description)
    {
        var plan = CreateValidPlan();
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));

        var command = new UpdatePlanCommand(
            "Plan Name",
            description!,
            price,
            BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var plan = CreateValidPlan();
        var price = _createMoney.Execute(new CreateMoneyCommand(10m, CurrencyVO.EUR));

        var command = new UpdatePlanCommand(
            "Plan Name",
            new string('a', 501),
            price,
            BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
