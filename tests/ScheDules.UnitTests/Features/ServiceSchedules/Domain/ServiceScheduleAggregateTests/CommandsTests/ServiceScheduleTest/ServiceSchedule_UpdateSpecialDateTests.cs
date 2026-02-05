namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_UpdateSpecialDateTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSpecialDateValidator _specialDateValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate;
    private readonly ServiceSchedule.UpdateSpecialDate _updateSpecialDate;

    public ServiceSchedule_UpdateSpecialDateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);

        var specialDateCreate = new ServiceSpecialDate.Create(_specialDateValidator);
        _addSpecialDate = new ServiceSchedule.AddSpecialDate(serviceCreate, specialDateCreate, _scheduleValidator);
        _updateSpecialDate = new ServiceSchedule.UpdateSpecialDate(serviceCreate, specialDateCreate, _scheduleValidator);
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

    private ServiceSchedule CreateScheduleWithDinnerAndSpecialDate()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Dinner,
            40,
            CreateWeekdaysSchedule()));
        _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            null,
            "San Valentín"));
        return schedule;
    }

    [Fact]
    public void Execute_WithUpdatedHours_UpdatesSpecialDate()
    {
        var schedule = CreateScheduleWithDinnerAndSpecialDate();

        var result = _updateSpecialDate.Execute(schedule, new UpdateServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(18, 0),
            new TimeOnly(23, 30),
            null,
            "San Valentín - Horario extendido"));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        var specialDate = service.SpecialDates.First(sd => sd.Date == new DateOnly(2025, 2, 14));
        specialDate.StartTime.Should().Be(new TimeOnly(18, 0));
        specialDate.EndTime.Should().Be(new TimeOnly(23, 30));
        specialDate.Reason.Should().Be("San Valentín - Horario extendido");
    }

    [Fact]
    public void Execute_ChangeFromAvailableToClosed_UpdatesSpecialDate()
    {
        var schedule = CreateScheduleWithDinnerAndSpecialDate();

        var result = _updateSpecialDate.Execute(schedule, new UpdateServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            false,
            null,
            null,
            null,
            "Cerrado por reformas"));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        var specialDate = service.SpecialDates.First(sd => sd.Date == new DateOnly(2025, 2, 14));
        specialDate.IsAvailable.Should().BeFalse();
        specialDate.StartTime.Should().BeNull();
        specialDate.EndTime.Should().BeNull();
        specialDate.Reason.Should().Be("Cerrado por reformas");
    }

    [Fact]
    public void Execute_WithCapacityOverride_UpdatesSpecialDate()
    {
        var schedule = CreateScheduleWithDinnerAndSpecialDate();

        var result = _updateSpecialDate.Execute(schedule, new UpdateServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0),
            60,
            "San Valentín con más capacidad"));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        var specialDate = service.SpecialDates.First(sd => sd.Date == new DateOnly(2025, 2, 14));
        specialDate.CapacityOverride.Should().Be(60);
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();

        var act = () => _updateSpecialDate.Execute(schedule, new UpdateServiceScheduleSpecialDateCommand(
            ServiceType.Breakfast,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(8, 0),
            new TimeOnly(11, 0)));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Service of type 'Breakfast' not found*");
    }

    [Fact]
    public void Execute_WhenSpecialDateNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Dinner,
            40,
            CreateWeekdaysSchedule()));

        var act = () => _updateSpecialDate.Execute(schedule, new UpdateServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 3, 15),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(23, 0)));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found for this service*");
    }

    [Fact]
    public void Execute_AvailableWithoutStartTime_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithDinnerAndSpecialDate();

        var act = () => _updateSpecialDate.Execute(schedule, new UpdateServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14),
            true,
            null,
            new TimeOnly(23, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceSpecialDateValidationMessages.StartTimeRequired}*");
    }
}
