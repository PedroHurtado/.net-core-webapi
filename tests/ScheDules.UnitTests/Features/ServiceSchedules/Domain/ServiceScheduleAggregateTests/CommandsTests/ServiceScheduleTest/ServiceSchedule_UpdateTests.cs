namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_UpdateTests
{
    private readonly ServiceScheduleValidator _validator = new();
    private readonly ReservationPolicyValidator _policyValidator = new();
    private readonly ServiceSchedule.Create _create;
    private readonly ServiceSchedule.Update _update;

    public ServiceSchedule_UpdateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(_policyValidator);
        _create = new ServiceSchedule.Create(reservationPolicyCreate, _validator);
        _update = new ServiceSchedule.Update(reservationPolicyCreate, _validator);
    }

    private ServiceSchedule CreateSchedule() => _create.Execute(new CreateServiceScheduleCommand(
        Guid.NewGuid(),
        "Horario Original",
        "Descripción original",
        TimeSpan.FromHours(2),
        TimeSpan.FromDays(30),
        TimeSpan.FromMinutes(15),
        TimeSpan.Zero,
        8,
        1,
        []));

    [Fact]
    public void Execute_WithValidData_UpdatesSchedule()
    {
        var schedule = CreateSchedule();

        var result = _update.Execute(schedule, new UpdateServiceScheduleCommand(
            "Horario Invierno",
            "Octubre a Mayo",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        result.Name.Should().Be("Horario Invierno");
        result.Description.Should().Be("Octubre a Mayo");
    }

    [Fact]
    public void Execute_WithUpdatedPolicy_UpdatesSchedule()
    {
        var schedule = CreateSchedule();

        var result = _update.Execute(schedule, new UpdateServiceScheduleCommand(
            "Horario Actualizado",
            "Con nueva policy",
            TimeSpan.FromHours(3),
            TimeSpan.FromDays(60),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(15),
            10,
            2,
            []));

        result.Policy.MinimumAdvanceTime.Should().Be(TimeSpan.FromHours(3));
        result.Policy.MaximumAdvanceTime.Should().Be(TimeSpan.FromDays(60));
        result.Policy.SlotInterval.Should().Be(TimeSpan.FromMinutes(30));
        result.Policy.BufferBetweenReservations.Should().Be(TimeSpan.FromMinutes(15));
        result.Policy.MaxPartySize.Should().Be(10);
        result.Policy.MinPartySize.Should().Be(2);
    }

    [Fact]
    public void Execute_WithUpdatedStandardDurations_UpdatesSchedule()
    {
        var schedule = CreateSchedule();
        var newDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromMinutes(45) },
            { ServiceType.Lunch, TimeSpan.FromHours(2) },
            { ServiceType.Dinner, TimeSpan.FromMinutes(150) }
        };

        var result = _update.Execute(schedule, new UpdateServiceScheduleCommand(
            "Horario con Duraciones",
            "Duraciones actualizadas",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            newDurations));

        result.Policy.StandardDurations.Should().HaveCount(3);
        result.Policy.GetDurationFor(ServiceType.Breakfast).Should().Be(TimeSpan.FromMinutes(45));
        result.Policy.GetDurationFor(ServiceType.Lunch).Should().Be(TimeSpan.FromHours(2));
        result.Policy.GetDurationFor(ServiceType.Dinner).Should().Be(TimeSpan.FromMinutes(150));
    }

    [Fact]
    public void Execute_PreservesOriginalId_WhenUpdating()
    {
        var schedule = CreateSchedule();
        var originalId = schedule.Id;

        var result = _update.Execute(schedule, new UpdateServiceScheduleCommand(
            "Nuevo Nombre",
            "Nueva Descripción",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        result.Id.Should().Be(originalId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var schedule = CreateSchedule();

        var act = () => _update.Execute(schedule, new UpdateServiceScheduleCommand(
            name!,
            "Description",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.NameRequired}*");
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _update.Execute(schedule, new UpdateServiceScheduleCommand(
            new string('a', 101),
            "Description",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.NameMaxLength}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyDescription_ThrowsValidationException(string? description)
    {
        var schedule = CreateSchedule();

        var act = () => _update.Execute(schedule, new UpdateServiceScheduleCommand(
            "Test",
            description!,
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.DescriptionRequired}*");
    }

    [Fact]
    public void Execute_WithInvalidPolicy_ThrowsValidationException()
    {
        var schedule = CreateSchedule();

        var act = () => _update.Execute(schedule, new UpdateServiceScheduleCommand(
            "Test",
            "Description",
            TimeSpan.FromHours(5),
            TimeSpan.FromHours(2),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.MaximumAdvanceTimeMustBeGreaterThanMinimum}*");
    }
}
