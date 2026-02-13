namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanCreateTests
{
    private readonly PlanValidator _validator = new();
    private readonly Plan.Create _create;

    public PlanCreateTests()
    {
        _create = new(_validator);
    }

    [Fact]
    public void Execute_WithValidCommand_ReturnsPlan()
    {
        var command = new CreatePlanCommand(
            Name: "Plan Básico",
            Description: "Plan ideal para empezar"
        );

        var result = _create.Execute(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Plan Básico");
        result.Description.Should().Be("Plan ideal para empezar");
        result.IsActive.Should().BeFalse();
        result.Features.Should().BeEmpty();
        result.PricingTiers.Should().BeEmpty();
    }

    #region Validation Throws

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var command = new CreatePlanCommand(
            Name: name!,
            Description: "Description"
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreatePlanCommand(
            Name: new string('a', 101),
            Description: "Description"
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
            Description: description!
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var command = new CreatePlanCommand(
            Name: "Plan Name",
            Description: new string('a', 501)
        );

        var act = () => _create.Execute(command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
