namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_UpdateServiceTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSpecialDateValidator _specialDateValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.UpdateService _updateService;
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate;

    public ServiceSchedule_UpdateServiceTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        var serviceAddSpecialDate = new Service.AddSpecialDate(_serviceValidator);

        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);
        _updateService = new ServiceSchedule.UpdateService(serviceCreate, serviceAddSpecialDate, _scheduleValidator);

        var specialDateCreate = new ServiceSpecialDate.Create(_specialDateValidator);
        _addSpecialDate = new ServiceSchedule.AddSpecialDate(specialDateCreate, serviceAddSpecialDate, _scheduleValidator);
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
        { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
        { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
        { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
        { DayOfWeek.Thursday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
        { DayOfWeek.Friday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) }
    };

    private ServiceSchedule CreateScheduleWithLunch()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));
        return schedule;
    }

    [Fact]
    public void Execute_WithUpdatedCapacity_UpdatesService()
    {
        var schedule = CreateScheduleWithLunch();

        var result = _updateService.Execute(schedule, new UpdateServiceCommand(
            ServiceType.Lunch,
            60,
            CreateWeekdaysSchedule()));

        var service = result.Services.First(s => s.Type == ServiceType.Lunch);
        service.MaxCapacity.Should().Be(60);
    }

    [Fact]
    public void Execute_WithUpdatedWeeklySchedule_UpdatesService()
    {
        var schedule = CreateScheduleWithLunch();
        var newSchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(17, 0)) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(17, 0)) },
            { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Thursday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(17, 0)) },
            { DayOfWeek.Friday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(17, 0)) }
        };

        var result = _updateService.Execute(schedule, new UpdateServiceCommand(
            ServiceType.Lunch,
            50,
            newSchedule));

        var service = result.Services.First(s => s.Type == ServiceType.Lunch);
        service.WeeklySchedule.Should().HaveCount(5);
        service.WeeklySchedule[DayOfWeek.Monday].StartTime.Should().Be(new TimeOnly(12, 0));
        service.WeeklySchedule[DayOfWeek.Wednesday].IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Execute_MaintainsExistingSpecialDates_WhenUpdating()
    {
        var schedule = CreateScheduleWithLunch();
        _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Lunch,
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(12, 0),
            new TimeOnly(18, 0),
            null,
            "San Valentín"));

        var result = _updateService.Execute(schedule, new UpdateServiceCommand(
            ServiceType.Lunch,
            60,
            CreateWeekdaysSchedule()));

        var service = result.Services.First(s => s.Type == ServiceType.Lunch);
        service.SpecialDates.Should().HaveCount(1);
        service.SpecialDates.First().Date.Should().Be(new DateOnly(2025, 2, 14));
        service.SpecialDates.First().Reason.Should().Be("San Valentín");
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();

        var act = () => _updateService.Execute(schedule, new UpdateServiceCommand(
            ServiceType.Breakfast,
            30,
            CreateWeekdaysSchedule()));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Service of type 'Breakfast' not found*");
    }

    [Fact]
    public void Execute_WithMaxCapacityZero_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithLunch();

        var act = () => _updateService.Execute(schedule, new UpdateServiceCommand(
            ServiceType.Lunch,
            0,
            CreateWeekdaysSchedule()));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithNoDaysAvailable_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithLunch();
        var unavailableSchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(false) }
        };

        var act = () => _updateService.Execute(schedule, new UpdateServiceCommand(
            ServiceType.Lunch,
            50,
            unavailableSchedule));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }
}
