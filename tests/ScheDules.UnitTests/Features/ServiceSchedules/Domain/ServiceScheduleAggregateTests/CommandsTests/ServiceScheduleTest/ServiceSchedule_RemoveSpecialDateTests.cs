namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_RemoveSpecialDateTests
{
    private readonly ServiceScheduleValidator _scheduleValidator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceDayConfigValidator _dayConfigValidator = new();
    private readonly ServiceValidator _serviceValidator = new();
    private readonly ServiceSpecialDateValidator _specialDateValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.AddService _addService;
    private readonly ServiceSchedule.AddSpecialDate _addSpecialDate;
    private readonly ServiceSchedule.RemoveSpecialDate _removeSpecialDate;

    public ServiceSchedule_RemoveSpecialDateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _scheduleValidator);

        var serviceDayConfigCreate = new ServiceDayConfig.Create(_dayConfigValidator);
        var serviceCreate = new Service.Create(serviceDayConfigCreate, _serviceValidator);
        _addService = new ServiceSchedule.AddService(serviceCreate, _scheduleValidator);

        var specialDateCreate = new ServiceSpecialDate.Create(_specialDateValidator);
        var serviceAddSpecialDate = new Service.AddSpecialDate(_serviceValidator);
        _addSpecialDate = new ServiceSchedule.AddSpecialDate(specialDateCreate, serviceAddSpecialDate, _scheduleValidator);

        var serviceRemoveSpecialDate = new Service.RemoveSpecialDate(_serviceValidator);
        _removeSpecialDate = new ServiceSchedule.RemoveSpecialDate(serviceRemoveSpecialDate, _scheduleValidator);
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
    public void Execute_WithExistingSpecialDate_RemovesSpecialDate()
    {
        var schedule = CreateScheduleWithDinnerAndSpecialDate();

        var result = _removeSpecialDate.Execute(schedule, new RemoveServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14)));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        service.SpecialDates.Should().BeEmpty();
        service.HasSpecialDates.Should().BeFalse();
    }

    [Fact]
    public void Execute_RemovesOnlySpecifiedDate_WhenMultipleExist()
    {
        var schedule = CreateScheduleWithDinnerAndSpecialDate();
        _addSpecialDate.Execute(schedule, new AddServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 12, 31),
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 59),
            null,
            "Nochevieja"));

        var result = _removeSpecialDate.Execute(schedule, new RemoveServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 2, 14)));

        var service = result.Services.First(s => s.Type == ServiceType.Dinner);
        service.SpecialDates.Should().HaveCount(1);
        service.SpecialDates.Should().NotContain(sd => sd.Date == new DateOnly(2025, 2, 14));
        service.SpecialDates.Should().Contain(sd => sd.Date == new DateOnly(2025, 12, 31));
    }

    [Fact]
    public void Execute_WhenServiceNotExists_ThrowsKeyNotFoundException()
    {
        var schedule = CreateSchedule();

        var act = () => _removeSpecialDate.Execute(schedule, new RemoveServiceScheduleSpecialDateCommand(
            ServiceType.Breakfast,
            new DateOnly(2025, 2, 14)));

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

        var act = () => _removeSpecialDate.Execute(schedule, new RemoveServiceScheduleSpecialDateCommand(
            ServiceType.Dinner,
            new DateOnly(2025, 3, 15)));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found for this service*");
    }
}
