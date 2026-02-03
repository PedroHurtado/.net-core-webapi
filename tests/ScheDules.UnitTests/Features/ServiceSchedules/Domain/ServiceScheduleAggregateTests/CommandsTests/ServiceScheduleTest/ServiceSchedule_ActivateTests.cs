namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_ActivateTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.Activate _activate;

    public ServiceSchedule_ActivateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);

        _activate = new ServiceSchedule.Activate(_scheduleValidator);
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

    private ServiceSchedule CreateScheduleWithService()
    {
        var schedule = CreateSchedule();
        _addService.Execute(schedule, new AddServiceCommand(
            ServiceType.Lunch,
            50,
            CreateWeekdaysSchedule()));
        return schedule;
    }

    [Fact]
    public void Execute_WithServicesConfigured_ActivatesSchedule()
    {
        var schedule = CreateScheduleWithService();

        var result = _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Execute_WithCurrentActive_DeactivatesPrevious()
    {
        var currentActive = CreateScheduleWithService();
        _activate.Execute(currentActive, new ActivateServiceScheduleCommand(null));
        var newSchedule = CreateScheduleWithService();

        var result = _activate.Execute(newSchedule, new ActivateServiceScheduleCommand(currentActive));

        result.IsActive.Should().BeTrue();
        currentActive.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Execute_WhenAlreadyActive_ThrowsConflictException()
    {
        var schedule = CreateScheduleWithService();
        _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

        var act = () => _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

        act.Should().Throw<ConflictException>()
            .WithMessage("*Service schedule is already active*");
    }

    [Fact]
    public void Execute_WithoutServices_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _activate.Execute(schedule, new ActivateServiceScheduleCommand(null));

        act.Should().Throw<ValidationException>()
            .WithMessage("*Service schedule must have at least one service*");
    }
}
