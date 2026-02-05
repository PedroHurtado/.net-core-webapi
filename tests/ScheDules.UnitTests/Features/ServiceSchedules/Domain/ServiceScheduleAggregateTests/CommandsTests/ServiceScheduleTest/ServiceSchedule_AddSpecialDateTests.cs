namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_AddSpecialDateTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSpecialDateValidator _specialDateValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate;

    public ServiceSchedule_AddSpecialDateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);

        var specialDateCreate = new ServiceSpecialDate.Create(_specialDateValidator);
        _addSpecialDate = new ServiceSchedule.AddSpecialDate(serviceCreate, specialDateCreate, _scheduleValidator);
    }

    private ServiceSchedule CreateSchedule() => _create.Execute(new CreateServiceScheduleCommand(
        Guid.NewGuid(),
        "Test Schedule",
        "Test Description",
        TimeSpan.FromHours(2),
        TimeSpan.FromDays(30),
        TimeSpan.FromMinutes(15),
        TimeSpan.Zero,
        8,
        1,
        []));

    private Dictionary<DayOfWeek, CreateServiceDayConfigCommand> CreateWeekdaysSchedule() => new()
    {
        { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
        { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
        { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) }
    };

    private ServiceSchedule CreateScheduleWithDinner()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Dinner,
            40,
            CreateWeekdaysSchedule()));
        return schedule;
    }

    [Fact]
    public void Execute_WithExtendedHours_AddsSpecialDate()
    {
        var schedule = CreateScheduleWithDinner();

        var result = _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 30),
            null,
            "San Valentín"));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        service.SpecialDates.Should().HaveCount(1);
        service.HasSpecialDates.Should().BeTrue();

        var specialDate = service.SpecialDates.First();
        specialDate.Date.Should().Be(new DateOnly(2025, 2, 14));
        specialDate.IsAvailable.Should().BeTrue();
        specialDate.StartTime.Should().Be(new TimeOnly(19, 0));
        specialDate.EndTime.Should().Be(new TimeOnly(23, 30));
        specialDate.Reason.Should().Be("San Valentín");
    }

    [Fact]
    public void Execute_WithClosedDate_AddsSpecialDate()
    {
        var schedule = CreateScheduleWithDinner();

        var result = _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 1, 1),
            false,
            null,
            null,
            null,
            "Año Nuevo"));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        var specialDate = service.SpecialDates.First();
        specialDate.IsAvailable.Should().BeFalse();
        specialDate.StartTime.Should().BeNull();
        specialDate.EndTime.Should().BeNull();
        specialDate.Reason.Should().Be("Año Nuevo");
    }

    [Fact]
    public void Execute_WithCapacityOverride_AddsSpecialDate()
    {
        var schedule = CreateScheduleWithDinner();

        var result = _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 12, 31),
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 59),
            70,
            "Nochevieja"));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        var specialDate = service.SpecialDates.First();
        specialDate.CapacityOverride.Should().Be(70);
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();

        var act = () => _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Breakfast,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(8, 0),
            new TimeOnly(11, 0)));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Service of type 'Breakfast' not found*");
    }

    [Fact]
    public void Execute_WithDuplicateDate_ThrowsConflictException()
    {
        var schedule = CreateScheduleWithDinner();
        _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            "San Valentín"));

        var act = () => _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            false,
            null,
            null,
            null,
            "Otro evento"));

        act.Should().Throw<ConflictException>()
            .WithMessage("*already exists for this service*");
    }

    [Fact]
    public void Execute_WithEmptyDate_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithDinner();

        var act = () => _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            default,
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.DateRequired}*");
    }

    [Fact]
    public void Execute_AvailableWithoutStartTime_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithDinner();

        var act = () => _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            null,
            new TimeOnly(23, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public void Execute_WithReasonExceedingMaxLength_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithDinner();

        var act = () => _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            new string('a', 201)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.ReasonMaxLength}*");
    }
}
