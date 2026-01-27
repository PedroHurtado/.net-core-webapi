namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanCreateTests
{
    private readonly PlanValidator _validator = new();
    private readonly MoneyValidator _moneyValidator = new();
    private readonly Money.Create _createMoney;
    private readonly Plan.Create _create;

    public PlanCreateTests()
    {
        _createMoney = new(_moneyValidator);
        _create = new(_createMoney, _validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsPlan()
    {
        var command = new CreatePlanCommand(
            Name: "Plan Básico",
            Description: "Plan ideal para empezar",
            Amount: 9.99m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Plan Básico");
        result.Description.Should().Be("Plan ideal para empezar");
        result.Price.Amount.Should().Be(9.99m);
        result.Price.Currency.Code.Should().Be("EUR");
        result.BillingPeriod.Should().Be(BillingPeriod.Monthly);
        result.IsActive.Should().BeFalse();
        result.Features.Should().BeEmpty();
        result.ProviderConfigurations.Should().BeEmpty();
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var command = new CreatePlanCommand(
            Name: name!,
            Description: "Description",
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreatePlanCommand(
            Name: new string('a', 101),
            Description: "Description",
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyDescription_ThrowsValidationException(string? description)
    {
        var command = new CreatePlanCommand(
            Name: "Plan Name",
            Description: description!,
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreatePlanCommand(
            Name: "Plan Name",
            Description: new string('a', 501),
            Amount: 10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNegativeAmount_ThrowsValidationException()
    {
        var command = new CreatePlanCommand(
            Name: "Plan Name",
            Description: "Description",
            Amount: -10m,
            CurrencyCode: "EUR",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithInvalidCurrencyCode_ThrowsArgumentException()
    {
        var command = new CreatePlanCommand(
            Name: "Plan Name",
            Description: "Description",
            Amount: 10m,
            CurrencyCode: "XXX",
            BillingPeriod: BillingPeriod.Monthly
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
