namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_ConfigureServiceDayTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.ConfigureServiceDay _configureServiceDay;

    public ServiceSchedule_ConfigureServiceDayTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);

        _configureServiceDay = new ServiceSchedule.ConfigureServiceDay(serviceCreate, _scheduleValidator);
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
    public void Execute_WithExtendedHoursAndCapacity_ConfiguresDay()
    {
        var schedule = CreateScheduleWithLunch();

        var result = _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Lunch,
            DayOfWeek.Friday,
            true,
            new TimeOnly(12, 0),
            new TimeOnly(17, 0),
            60));

        var service = result.Services.First(s => s.Type == ServiceType.Lunch);
        var fridayConfig = service.WeeklySchedule[DayOfWeek.Friday];
        fridayConfig.IsAvailable.Should().BeTrue();
        fridayConfig.StartTime.Should().Be(new TimeOnly(12, 0));
        fridayConfig.EndTime.Should().Be(new TimeOnly(17, 0));
        fridayConfig.CapacityOverride.Should().Be(60);
    }

    [Fact]
    public void Execute_WithUnavailableDay_ConfiguresDay()
    {
        var schedule = CreateScheduleWithLunch();

        var result = _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Lunch,
            DayOfWeek.Friday,
            false));

        var service = result.Services.First(s => s.Type == ServiceType.Lunch);
        var fridayConfig = service.WeeklySchedule[DayOfWeek.Friday];
        fridayConfig.IsAvailable.Should().BeFalse();
        fridayConfig.StartTime.Should().BeNull();
        fridayConfig.EndTime.Should().BeNull();
    }

    [Fact]
    public void Execute_AddsNewDayToSchedule()
    {
        var schedule = CreateScheduleWithLunch();

        var result = _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Lunch,
            DayOfWeek.Saturday,
            true,
            new TimeOnly(12, 0),
            new TimeOnly(15, 0)));

        var service = result.Services.First(s => s.Type == ServiceType.Lunch);
        service.WeeklySchedule.Should().ContainKey(DayOfWeek.Saturday);
        service.WeeklySchedule[DayOfWeek.Saturday].IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();

        var act = () => _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Breakfast,
            DayOfWeek.Monday,
            true,
            new TimeOnly(8, 0),
            new TimeOnly(11, 0)));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Service of type 'Breakfast' not found*");
    }

    [Fact]
    public void Execute_AvailableWithoutStartTime_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithLunch();

        var act = () => _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Lunch,
            DayOfWeek.Friday,
            true,
            null,
            new TimeOnly(16, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.StartTimeRequired}*");
    }

    [Fact]
    public void Execute_AvailableWithoutEndTime_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithLunch();

        var act = () => _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Lunch,
            DayOfWeek.Friday,
            true,
            new TimeOnly(13, 0),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeRequired}*");
    }

    [Fact]
    public void Execute_WithEndTimeBeforeStartTime_ThrowsValidationException()
    {
        var schedule = CreateScheduleWithLunch();

        var act = () => _configureServiceDay.Execute(schedule, new ConfigureServiceDayCommand(
            ServiceType.Lunch,
            DayOfWeek.Friday,
            true,
            new TimeOnly(16, 0),
            new TimeOnly(13, 0)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceDayConfigValidationMessages.EndTimeMustBeAfterStartTime}*");
    }
}
