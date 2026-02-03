namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_AddSpecialDateTests
{
    private readonly ScheduleValidator _validator = new();
    private readonly TimeSlotValidator _timeSlotValidator = new();
    private readonly SpecialDateValidator _specialDateValidator = new();
    private readonly Schedule.Create _create;
    private readonly Schedule.AddSpecialDate _addSpecialDate;

    public Schedule_AddSpecialDateTests()
    {
        _create = new(_validator);
        var timeSlotCreate = new TimeSlot.Create(_timeSlotValidator);
        var specialDateCreate = new SpecialDate.Create(timeSlotCreate, _specialDateValidator);
        _addSpecialDate = new(specialDateCreate, _validator);
    }

    private Schedule CreateSchedule() => _create.Execute(new CreateScheduleCommand(
        Guid.NewGuid(),
        "Test Schedule",
        null));

    [Fact]
    public void Execute_WithClosedDate_AddsSpecialDate()
    {
        var schedule = CreateSchedule();

        var result = _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []));

        result.SpecialDates.Should().HaveCount(1);
        var specialDate = result.SpecialDates.First();
        specialDate.Date.Should().Be(new DateOnly(2025, 12, 25));
        specialDate.IsClosed.Should().BeTrue();
        specialDate.Reason.Should().Be("Navidad");
        specialDate.TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithSpecialHours_AddsSpecialDate()
    {
        var schedule = CreateSchedule();

        var result = _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 2, 14),
            false,
            "San Valentín",
            [new CreateTimeSlotCommand(new TimeOnly(13, 0), new TimeOnly(23, 59))]));

        result.SpecialDates.Should().HaveCount(1);
        var specialDate = result.SpecialDates.First();
        specialDate.IsClosed.Should().BeFalse();
        specialDate.TimeSlots.Should().HaveCount(1);
    }

    [Fact]
    public void Execute_WithMultipleSpecialDates_AddsAll()
    {
        var schedule = CreateSchedule();
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []));

        var result = _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 31),
            false,
            "Nochevieja",
            [new CreateTimeSlotCommand(new TimeOnly(20, 0), new TimeOnly(23, 59))]));

        result.SpecialDates.Should().HaveCount(2);
    }

    [Fact]
    public void Execute_WithDuplicateDate_ThrowsConflictException()
    {
        var schedule = CreateSchedule();
        _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "Navidad",
            []));

        var act = () => _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            false,
            "Otro evento",
            []));

        act.Should().Throw<ConflictException>()
            .WithMessage("*Special date for '*' already exists*");
    }

    [Fact]
    public void Execute_WithEmptyReason_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _addSpecialDate.Execute(schedule, new AddSpecialDateCommand(
            new DateOnly(2025, 12, 25),
            true,
            "",
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{SpecialDateValidationMessages.ReasonRequired}*");
    }
}
