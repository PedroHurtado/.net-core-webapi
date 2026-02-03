namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_UpdateSpecialDateTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly SpecialDateValidator _specialDateValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.AddSpecialDate _addSpecialDate;
    private readonly Schedule.UpdateSpecialDate _updateSpecialDate;

    public Schedule_UpdateSpecialDateTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var specialDateCreate = new SpecialDate.Create(timeSlotCreate, _specialDateValidator);
        _addSpecialDate = new(specialDateCreate, _validator);
        _updateSpecialDate = new(specialDateCreate, _validator);
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
    public void Execute_WithValidData_UpdatesSpecialDate()
    {
        var schedule = CreateScheduleWithSpecialDate();

        var result = _updateSpecialDate.Execute(schedule, new UpdateSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            false,
            "Navidad (horario especial)",
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(18, 0))]));

        result.SpecialDates.Should().HaveCount(1);
        var updated = result.SpecialDates.First();
        updated.IsClosed.Should().BeFalse();
        updated.Reason.Should().Be("Navidad (horario especial)");
        updated.TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_FromOpenToClosed_UpdatesSpecialDate()
    {
        var schedule = _create.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            "Test Schedule",
            null));
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentín",
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 0))]));

        var result = _updateSpecialDate.Execute(schedule, new UpdateSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            true,
            "San Valentín cancelado",
            []));

        var updated = result.SpecialDates.First();
        updated.IsClosed.Should().BeTrue();
        updated.TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithNonExistentDate_ThrowsNotFoundException()
    {
        var schedule = CreateScheduleWithSpecialDate();

        var act = () => _updateSpecialDate.Execute(schedule, new UpdateSpecialDateCommand(
            new DateOnly(2025, 8, 15),
            true,
            "Vacaciones",
            []));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Special date for '*' not found*");
    }
}
