namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceScheduleTest;

public class ServiceSchedule_CreateTests
{
    private readonly ServiceSchedule.Create _command;

    public ServiceSchedule_CreateTests()
    {
        var reservationPolicyCreate = new ReservationPolicy.Create(new ReservationPolicyValidator());
        _command = new ServiceSchedule.Create(reservationPolicyCreate, new ServiceScheduleValidator());
    }

    [Fact]
    public void Execute_WithValidData_ReturnsServiceSchedule()
    {
        var tenantId = Guid.NewGuid();

        var result = _command.Execute(new CreateServiceScheduleCommand(
            tenantId,
            "Horario Verano",
            "Junio a Septiembre",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Horario Verano");
        result.Description.Should().Be("Junio a Septiembre");
        result.IsActive.Should().BeFalse();
        result.Services.Should().BeEmpty();
        result.HasServices.Should().BeFalse();
        result.ServiceCount.Should().Be(0);
    }

    [Fact]
    public void Execute_WithStandardDurations_ReturnsServiceSchedule()
    {
        var durations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromHours(1) },
            { ServiceType.Lunch, TimeSpan.FromMinutes(90) },
            { ServiceType.Dinner, TimeSpan.FromHours(2) }
        };

        var result = _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
            "Horario Completo",
            "Con duraciones por servicio",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            durations));

        result.Policy.StandardDurations.Should().HaveCount(3);
        result.Policy.GetDurationFor(ServiceType.Breakfast).Should().Be(TimeSpan.FromHours(1));
        result.Policy.GetDurationFor(ServiceType.Lunch).Should().Be(TimeSpan.FromMinutes(90));
        result.Policy.GetDurationFor(ServiceType.Dinner).Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Execute_WithBufferBetweenReservations_ReturnsServiceSchedule()
    {
        var result = _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
            "Horario con Buffer",
            "15 minutos entre reservas",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(15),
            8,
            1,
            []));

        result.Policy.BufferBetweenReservations.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void Execute_WithEmptyTenantId_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.Empty,
            "Test",
            "Description",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.TenantIdRequired}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
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
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
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
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
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
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
            "Test",
            new string('a', 501),
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceScheduleValidationMessages.DescriptionMaxLength}*");
    }

    [Fact]
    public void Execute_WithInvalidPolicy_MaxAdvanceLessThanMinAdvance_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
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

    [Fact]
    public void Execute_WithInvalidSlotInterval_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceScheduleCommand(
            Guid.NewGuid(),
            "Test",
            "Description",
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(7),
            TimeSpan.Zero,
            8,
            1,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ReservationPolicyValidationMessages.SlotIntervalMustBeValid}*");
    }
}
