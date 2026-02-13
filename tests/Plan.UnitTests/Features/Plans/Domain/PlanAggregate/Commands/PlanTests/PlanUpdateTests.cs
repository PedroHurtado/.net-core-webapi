namespace Plans.UnitTests.Features.Plans.Domain.PlanAggregate.Commands.PlanTests;

public class PlanUpdateTests
{
    private readonly PlanValidator _validator = new();
    private readonly Plan.Update _update;

    public PlanUpdateTests()
    {
        _update = new(_validator);
    }

    private TestablePlan CreateValidPlan()
    {
        var plan = new TestablePlan(Guid.NewGuid());
        plan.SetName("Original Plan");
        plan.SetDescription("Original description");
        plan.SetIsActive(true);
        return plan;
    }

    [Fact]
    public void Execute_WithValidCommand_UpdatesPlan()
    {
        var plan = CreateValidPlan();
        var command = new UpdatePlanCommand(
            Name: "Updated Plan",
            Description: "Updated description"
        );

        var result = _update.Execute(plan, command);

        result.Name.Should().Be("Updated Plan");
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public void Execute_PreservesNonUpdatedProperties()
    {
        var plan = CreateValidPlan();
        var originalId = plan.Id;
        var originalIsActive = plan.IsActive;

        var command = new UpdatePlanCommand(
            Name: "New Name",
            Description: "New description"
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
            Description: "Description"
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
            Description: "Description"
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
            Description: description!
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
            Description: new string('a', 501)
        );

        var act = () => _update.Execute(plan, command);

        act.Should().Throw<ValidationException>();
    }

    #endregion
}
