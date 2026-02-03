namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_UpdateTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.Update _update;

    public Schedule_UpdateTests()
    {
        _create = new(_validator);
        _update = new(_validator);
    }

    private Schedule CreateSchedule() => _create.Execute(new CreateScheduleCommand(
        Guid.NewGuid(),
        "Original Name",
        "Original Description"));

    [Fact]
    public void Execute_WithValidData_UpdatesSchedule()
    {
        var schedule = CreateSchedule();

        var result = _update.Execute(schedule, new UpdateScheduleCommand(
            "Horario de Verano Actualizado",
            "Nueva descripción"));

        result.Name.Should().Be("Horario de Verano Actualizado");
        result.Description.Should().Be("Nueva descripción");
    }

    [Fact]
    public void Execute_WithNullDescription_ClearsDescription()
    {
        var schedule = CreateSchedule();

        var result = _update.Execute(schedule, new UpdateScheduleCommand(
            "Updated Name",
            null));

        result.Name.Should().Be("Updated Name");
        result.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var schedule = CreateSchedule();

        var act = () => _update.Execute(schedule, new UpdateScheduleCommand(
            name!,
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ScheduleValidationMessages.NameRequired}*");
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _update.Execute(schedule, new UpdateScheduleCommand(
            new string('a', 101),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ScheduleValidationMessages.NameMaxLength}*");
    }
}
