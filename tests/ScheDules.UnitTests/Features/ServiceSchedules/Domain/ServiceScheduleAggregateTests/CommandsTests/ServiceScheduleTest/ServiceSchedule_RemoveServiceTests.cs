namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_RemoveServiceTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.RemoveService _removeService;

    public ServiceSchedule_RemoveServiceTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);
        _removeService = new ServiceSchedule.RemoveService(_scheduleValidator);
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
        { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) }
    };

    [Fact]
    public void Execute_WithExistingService_RemovesService()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(ServiceType.Lunch, 50, CreateWeekdaysSchedule()));
        _addService.Execute(schedule, new AddServiceCommand(ServiceType.Dinner, 40, CreateWeekdaysSchedule()));

        var result = _removeService.Execute(schedule, new RemoveServiceCommand(ServiceType.Lunch));

        result.Services.Should().HaveCount(1);
        result.AvailableServiceTypes.Should().NotContain(ServiceType.Lunch);
        result.AvailableServiceTypes.Should().Contain(ServiceType.Dinner);
    }

    [Fact]
    public void Execute_WithLastService_RemovesService()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(ServiceType.Lunch, 50, CreateWeekdaysSchedule()));

        var result = _removeService.Execute(schedule, new RemoveServiceCommand(ServiceType.Lunch));

        result.Services.Should().BeEmpty();
        result.HasServices.Should().BeFalse();
        result.ServiceCount.Should().Be(0);
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();

        var act = () => _removeService.Execute(schedule, new RemoveServiceCommand(ServiceType.Breakfast));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*Service of type 'Breakfast' not found*");
    }

    [Fact]
    public void Execute_RemovesCorrectService_WhenMultipleExist()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(ServiceType.Breakfast, 30, CreateWeekdaysSchedule()));
        _addService.Execute(schedule, new AddServiceCommand(ServiceType.Lunch, 50, CreateWeekdaysSchedule()));
        _addService.Execute(schedule, new AddServiceCommand(ServiceType.Dinner, 40, CreateWeekdaysSchedule()));

        var result = _removeService.Execute(schedule, new RemoveServiceCommand(ServiceType.Lunch));

        result.Services.Should().HaveCount(2);
        result.AvailableServiceTypes.Should().Contain(ServiceType.Breakfast);
        result.AvailableServiceTypes.Should().Contain(ServiceType.Dinner);
        result.AvailableServiceTypes.Should().NotContain(ServiceType.Lunch);
    }
}
