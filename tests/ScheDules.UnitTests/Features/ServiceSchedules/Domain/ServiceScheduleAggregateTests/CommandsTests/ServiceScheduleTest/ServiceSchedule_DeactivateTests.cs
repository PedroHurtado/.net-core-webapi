namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_DeactivateTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.Activate _activate;
    private readonly ServiceSchedule.Deactivate _deactivate;

    public ServiceSchedule_DeactivateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);

        _activate = new ServiceSchedule.Activate(_scheduleValidator);
        _deactivate = new ServiceSchedule.Deactivate(_scheduleValidator);
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

    private ServiceSchedule CreateActiveSchedule()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));
        _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));
        return schedule;
    }

    [Fact]
    public void Execute_WithActiveSchedule_DeactivatesSchedule()
    {
        var schedule = CreateActiveSchedule();

        var result = _deactivate.Execute(schedule);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyInactive_ThrowsConflictException()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));

        var act = () => _deactivate.Execute(schedule);

        act.Should().Throw<ConflictException>()
            .WithMessage("*Service schedule is already inactive*");
    }
}
