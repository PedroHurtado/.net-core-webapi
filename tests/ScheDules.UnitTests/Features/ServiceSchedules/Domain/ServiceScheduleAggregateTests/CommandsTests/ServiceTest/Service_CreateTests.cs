namespace ScheDules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests.CommandsTests.ServiceTest;

public class Service_CreateTests
{
    private readonly Service.Create _command;

    public Service_CreateTests()
    {
        var serviceDayConfigCreate = new ServiceDayConfig.Create(new ServiceDayConfigValidator());
        _command = new Service.Create(serviceDayConfigCreate, new ServiceValidator());
    }

    [Fact]
    public void Execute_WithWeekdaysSchedule_ReturnsService()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
            { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
            { DayOfWeek.Thursday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) },
            { DayOfWeek.Friday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) }
        };

        var result = _command.Execute(new CreateServiceCommand(
            ServiceType.Lunch,
            50,
            weeklySchedule));

        result.Type.Should().Be(ServiceType.Lunch);
        result.MaxCapacity.Should().Be(50);
        result.WeeklySchedule.Should().HaveCount(5);
        result.AvailableDaysCount.Should().Be(5);
    }

    [Fact]
    public void Execute_WithWeekendsOnlySchedule_ReturnsService()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Saturday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) },
            { DayOfWeek.Sunday, new CreateServiceDayConfigCommand(true, new TimeOnly(20, 0), new TimeOnly(23, 0)) }
        };

        var result = _command.Execute(new CreateServiceCommand(
            ServiceType.Dinner,
            40,
            weeklySchedule));

        result.Type.Should().Be(ServiceType.Dinner);
        result.MaxCapacity.Should().Be(40);
        result.WeeklySchedule.Should().HaveCount(2);
        result.AvailableDaysCount.Should().Be(2);
    }

    [Fact]
    public void Execute_WithMixedAvailability_ReturnsService()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(7, 0), new TimeOnly(10, 0)) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(true, new TimeOnly(7, 0), new TimeOnly(10, 0)) },
            { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Thursday, new CreateServiceDayConfigCommand(true, new TimeOnly(7, 0), new TimeOnly(10, 0)) },
            { DayOfWeek.Friday, new CreateServiceDayConfigCommand(true, new TimeOnly(7, 0), new TimeOnly(10, 0)) },
            { DayOfWeek.Saturday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Sunday, new CreateServiceDayConfigCommand(false) }
        };

        var result = _command.Execute(new CreateServiceCommand(
            ServiceType.Breakfast,
            30,
            weeklySchedule));

        result.Type.Should().Be(ServiceType.Breakfast);
        result.WeeklySchedule.Should().HaveCount(7);
        result.AvailableDaysCount.Should().Be(4);
    }

    [Fact]
    public void Execute_WithSingleAvailableDay_ReturnsService()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Sunday, new CreateServiceDayConfigCommand(true, new TimeOnly(12, 0), new TimeOnly(15, 0)) }
        };

        var result = _command.Execute(new CreateServiceCommand(
            ServiceType.Lunch,
            25,
            weeklySchedule));

        result.AvailableDaysCount.Should().Be(1);
        result.HasSpecialDates.Should().BeFalse();
    }

    [Fact]
    public void Execute_WithMaxCapacityZero_ThrowsValidationException()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) }
        };

        var act = () => _command.Execute(new CreateServiceCommand(
            ServiceType.Lunch,
            0,
            weeklySchedule));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithNegativeMaxCapacity_ThrowsValidationException()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(true, new TimeOnly(13, 0), new TimeOnly(16, 0)) }
        };

        var act = () => _command.Execute(new CreateServiceCommand(
            ServiceType.Lunch,
            -1,
            weeklySchedule));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.MaxCapacityMustBeGreaterThanZero}*");
    }

    [Fact]
    public void Execute_WithNoDaysAvailable_ThrowsValidationException()
    {
        var weeklySchedule = new Dictionary<DayOfWeek, CreateServiceDayConfigCommand>
        {
            { DayOfWeek.Monday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Tuesday, new CreateServiceDayConfigCommand(false) },
            { DayOfWeek.Wednesday, new CreateServiceDayConfigCommand(false) }
        };

        var act = () => _command.Execute(new CreateServiceCommand(
            ServiceType.Lunch,
            50,
            weeklySchedule));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }

    [Fact]
    public void Execute_WithEmptyWeeklySchedule_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateServiceCommand(
            ServiceType.Dinner,
            40,
            []));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ServiceValidationMessages.ServiceMustBeAvailableAtLeastOneDay}*");
    }
}
