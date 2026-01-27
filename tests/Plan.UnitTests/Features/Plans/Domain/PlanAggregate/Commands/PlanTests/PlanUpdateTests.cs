namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanUpdateTests
{
    private readonly PlanValidator _validator = new();
    private readonly MoneyValidator _moneyValidator = new();
    private readonly Money.Create _createMoney;
    private readonly Plan.Update _update;

    public PlanUpdateTests()
    {
        _createMoney = new(_moneyValidator);
        _update = new(_createMoney, _validator);
    }

    private TestablePlan CreateValidPlan()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Original Plan");
        plan.SetDescription("Original description");
        plan.SetPrice(_createMoney.Execute(new CreateMoneyCommand(10m, Currency.EUR)));
        plan.SetBillingPeriod(BillingPeriod.Monthly);
        plan.SetIsActive(true);
        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_UpdatesPlan()
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: "Updated Plan",
            Description: "Updated description",
            Amount: 20m,
            CurrencyCode: "USD",
            BillingPeriod: BillingPeriod.Yearly
        );

        var result = _update.Execute(plan, command);

        result.Name.Should().Be("Updated Plan");
        result.Description.Should().Be("Updated description");
        result.Price.Amount.Should().Be(20m);
        result.Price.Currency.Code.Should().Be("USD");
        result.BillingPeriod.Should().Be(BillingPeriod.Yearly);
    }

    [Fact]
    public void Execute_WithDifferentBillingPeriod_UpdatesBillingPeriod()
    {
        var plan = CreateValidPlan();
        plan.BillingPeriod.Should().Be(BillingPeriod.Monthly);

        var command = new UpdatePlanCommand(
            Name: "Plan Name",
            Description: "Description",
            Amount: 15m,
            CurrencyCode: "GBP",
            BillingPeriod: BillingPeriod.Quarterly
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

        var command = new UpdatePlanCommand(
            Name: "New Name",
            Description: "New description",
            Amount: 15m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var result = _update.Execute(plan, command);

        result.Id.Should().Be(originalId);
        result.IsActive.Should().Be(originalIsActive);
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: name!,
            Description: "Description",
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: new string('a', 101),
            Description: "Description",
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
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
        var command = new UpdatePlanCommand(
            Name: "Plan Name",
            Description: description!,
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: "Plan Name",
            Description: new string('a', 501),
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeAmount_ThrowsValidationException()
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: "Plan Name",
            Description: "Description",
            Amount: -10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithInvalidCurrencyCode_ThrowsArgumentException()
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: "Plan Name",
            Description: "Description",
            Amount: 10m,
            CurrencyCode: "XXX",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
