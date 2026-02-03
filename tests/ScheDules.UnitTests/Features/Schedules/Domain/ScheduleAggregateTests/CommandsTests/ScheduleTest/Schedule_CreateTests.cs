namespace ScheDules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests.CommandsTests.ScheduleTest;

public class Schedule_CreateTests
{
    private readonly Schedule.Create _command = new(new ScheduleValidator());

    [Fact]
    public void Execute_WithValidData_ReturnsSchedule()
    {
        var tenantId = Guid.NewGuid();

        var result = _command.Execute(new CreateScheduleCommand(
            tenantId,
            "Horario de Verano",
            "Del 15 junio al 15 septiembre"));

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be("Horario de Verano");
        result.Description.Should().Be("Del 15 junio al 15 septiembre");
        result.IsActive.Should().BeFalse();
        result.WeeklyHours.Should().BeEmpty();
        result.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public void Execute_WithoutDescription_ReturnsSchedule()
    {
        var tenantId = Guid.NewGuid();

        var result = _command.Execute(new CreateScheduleCommand(
            tenantId,
            "Horario de Invierno",
            null));

        result.Name.Should().Be("Horario de Invierno");
        result.Description.Should().BeNull();
    }

    [Fact]
    public void Execute_WithEmptyTenantId_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateScheduleCommand(
            Guid.Empty,
            "Test",
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ScheduleValidationMessages.TenantIdRequired}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Execute_WithEmptyName_ThrowsValidationException(string? name)
    {
        var act = () => _command.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            name!,
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ScheduleValidationMessages.NameRequired}*");
    }

    [Fact]
    public void Execute_WithNameExceedingMaxLength_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            new string('a', 101),
            null));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ScheduleValidationMessages.NameMaxLength}*");
    }

    [Fact]
    public void Execute_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var act = () => _command.Execute(new CreateScheduleCommand(
            Guid.NewGuid(),
            "Test",
            new string('a', 501)));

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{ScheduleValidationMessages.DescriptionMaxLength}*");
    }
}
