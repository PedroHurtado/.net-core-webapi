namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_RemoveSpecialDateTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly SpecialDateValidator _specialDateValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.AddSpecialDate _addSpecialDate;
    private readonly Schedule.RemoveSpecialDate _removeSpecialDate;

    public Schedule_RemoveSpecialDateTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var specialDateCreate = new SpecialDate.Create(timeSlotCreate, _specialDateValidator);
        _addSpecialDate = new(specialDateCreate, _validator);
        _removeSpecialDate = new(_validator);
    }

    private Schedule CreateScheduleWithSpecialDate()
    {
        var schedule = _create.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            "Test Schedule",
            null));
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []));
        return schedule;
    }

    [Fact]
    public void Execute_WithExistingDate_RemovesSpecialDate()
    {
        var schedule = CreateScheduleWithSpecialDate();

        var result = _removeSpecialDate.Execute(schedule, new RemoveSpecialDateCommand(
            new DateOnly(2025, 12, 25)));

        result.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithMultipleDates_RemovesOnlySpecified()
    {
        var schedule = _create.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            "Test Schedule",
            null));
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []));
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 31),
            false,
            "Nochevieja",
            [new CreateTimeSlotCommand(new TimeOnly(20, 0), new TimeOnly(23, 59))]));
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 1, 1),
            true,
            "Año Nuevo",
            []));

        var result = _removeSpecialDate.Execute(schedule, new RemoveSpecialDateCommand(
            new DateOnly(2025, 12, 25)));

        result.SpecialDates.Should().HaveCount(2);
        result.SpecialDates.Should().NotContain(sd => sd.Date == new DateOnly(2025, 12, 25));
    }

    [Fact]
    public void Execute_WithNonExistentDate_ThrowsNotFoundException()
    {
        var schedule = CreateScheduleWithSpecialDate();

        var act = () => _removeSpecialDate.Execute(schedule, new RemoveSpecialDateCommand(
            new DateOnly(2025, 8, 15)));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Special date for '*' not found*");
    }
}
