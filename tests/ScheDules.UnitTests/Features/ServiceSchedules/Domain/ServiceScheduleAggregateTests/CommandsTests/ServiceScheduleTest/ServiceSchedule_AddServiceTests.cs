namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_AddServiceTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;

    public ServiceSchedule_AddServiceTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);
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

    [Fact]
    public void Execute_WithFirstService_AddsService()
    {
        var schedule = CreateSchedule();

        var result = _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));

        result.Services.Should().HaveCount(1);
        result.HasServices.Should().BeTrue();
        result.ServiceCount.Should().Be(1);
        result.AvailableServiceTypes.Should().Contain(ServiceType.Lunch);
    }

    [Fact]
    public void Execute_WithSecondService_AddsService()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));

        var dinnerSchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
            { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
            { DayOfWeek.Thursday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
            { DayOfWeek.Friday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
            { DayOfWeek.Saturday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 30)) },
            { DayOfWeek.Sunday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(22, 30)) }
        };

        var result = _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Dinner,
            40,
            dinnerSchedule));

        result.Services.Should().HaveCount(2);
        result.ServiceCount.Should().Be(2);
        result.AvailableServiceTypes.Should().Contain(ServiceType.Lunch);
        result.AvailableServiceTypes.Should().Contain(ServiceType.Dinner);
    }

    [Fact]
    public void Execute_WithWeekendsOnlySchedule_AddsService()
    {
        var schedule = CreateSchedule();
        var weekendSchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Saturday, new CreateServiceDayConfigCommand(true, new TimeOnly(8, 0), new TimeOnly(11, 0)) },
            { DayOfWeek.Sunday, new CreateServiceDayConfigCommand(true, new TimeOnly(8, 0), new TimeOnly(11, 0)) }
        };

        var result = _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Breakfast,
            30,
            weekendSchedule));

        result.Services.Should().HaveCount(1);
        var service = result.Services.First();
        service.Type.Should().Be(ServiceType.Breakfast);
        service.AvailableDaysCount.Should().Be(2);
    }

    [Fact]
    public void Execute_WithDuplicateServiceType_ThrowsConflictException()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));

        var act = () => _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            60,
            CreateWeekdaysSchedule()));

        act.Should().Throw<ConflictException>()
            .WithMessage("*Service of type 'Lunch' already exists*");
    }

    [Fact]
    public void Execute_WithMaxCapacityZero_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            0,
            CreateWeekdaysSchedule()));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithNoDaysAvailable_ThrowsValidationException()
    {
        var schedule = CreateSchedule();
        var unavailableSchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(false) }
        };

        var act = () => _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            unavailableSchedule));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }

    [Fact]
    public void Execute_WithEmptyWeeklySchedule_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Dinner,
            40,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }
}
