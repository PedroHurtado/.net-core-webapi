namespace Schedules.UnitTests.Features.Schedules.Domain.ScheduleAggregateTests;

public class ScheduleTests
{
    #region Properties

    [Fact]
    public void Schedule_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var schedule = new TestableSchedule(id);

        // Act
        schedule.SetTenantId(tenantId);
        schedule.SetName("Horario de Verano");
        schedule.SetDescription("Del 15 junio al 15 septiembre");
        schedule.SetIsActive(false);

        // Assert
        schedule.Id.Should().Be(id);
        schedule.TenantId.Should().Be(tenantId);
        schedule.Name.Should().Be("Horario de Verano");
        schedule.Description.Should().Be("Del 15 junio al 15 septiembre");
        schedule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Schedule_WithDescription_ShouldSetCorrectly()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var description = "Descripción del horario";

        // Act
        schedule.SetDescription(description);

        // Assert
        schedule.Description.Should().Be(description);
    }

    [Fact]
    public void Schedule_WithoutDescription_ShouldBeNull()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());

        // Act
        schedule.SetDescription(null);

        // Assert
        schedule.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Schedule_IsActive_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());

        // Act
        schedule.SetIsActive(value);

        // Assert
        schedule.IsActive.Should().Be(value);
    }

    #endregion

    #region Computed Properties

    [Fact]
    public void HasWeeklyHours_WithWeeklyHours_ShouldReturnTrue()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, []);

        // Act
        schedule.AddWeeklyHours(DayOfWeek.Monday, daySchedule);

        // Assert
        schedule.HasWeeklyHours.Should().BeTrue();
    }

    [Fact]
    public void HasWeeklyHours_WithoutWeeklyHours_ShouldReturnFalse()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());

        // Act
        // No weekly hours added

        // Assert
        schedule.HasWeeklyHours.Should().BeFalse();
    }

    [Fact]
    public void HasSpecialDates_WithSpecialDates_ShouldReturnTrue()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var specialDate = new TestableSpecialDate(new DateOnly(2025, 12, 25), true, "Navidad", []);

        // Act
        schedule.SetSpecialDate(specialDate);

        // Assert
        schedule.HasSpecialDates.Should().BeTrue();
    }

    [Fact]
    public void HasSpecialDates_WithoutSpecialDates_ShouldReturnFalse()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());

        // Act
        // No special dates added

        // Assert
        schedule.HasSpecialDates.Should().BeFalse();
    }

    [Fact]
    public void IsFullyConfigured_WithAllDays_ShouldReturnTrue()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, []);

        // Act
        schedule.AddWeeklyHours(DayOfWeek.Monday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Tuesday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Wednesday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Thursday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Friday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Saturday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Sunday, daySchedule);

        // Assert
        schedule.IsFullyConfigured.Should().BeTrue();
    }

    [Fact]
    public void IsFullyConfigured_WithPartialDays_ShouldReturnFalse()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, []);

        // Act
        schedule.AddWeeklyHours(DayOfWeek.Monday, daySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Tuesday, daySchedule);

        // Assert
        schedule.IsFullyConfigured.Should().BeFalse();
    }

    #endregion

    #region Collections

    [Fact]
    public void WeeklyHours_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var daySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, []);

        // Act
        schedule.AddWeeklyHours(DayOfWeek.Monday, daySchedule);

        // Assert
        schedule.WeeklyHours.Should().BeAssignableTo<IReadOnlyDictionary<DayOfWeek, DaySchedule>>();
        schedule.WeeklyHours.Should().ContainSingle();
    }

    [Fact]
    public void SpecialDates_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var specialDate = new TestableSpecialDate(new DateOnly(2025, 12, 25), true, "Navidad", []);

        // Act
        schedule.SetSpecialDate(specialDate);

        // Assert
        schedule.SpecialDates.Should().BeAssignableTo<IReadOnlyCollection<SpecialDate>>();
        schedule.SpecialDates.Should().ContainSingle();
    }

    [Fact]
    public void Schedule_WithMultipleWeeklyHours_ShouldContainAllDays()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var mondaySchedule = new TestableDaySchedule(DayOfWeek.Monday, false, []);
        var tuesdaySchedule = new TestableDaySchedule(DayOfWeek.Tuesday, false, []);

        // Act
        schedule.AddWeeklyHours(DayOfWeek.Monday, mondaySchedule);
        schedule.AddWeeklyHours(DayOfWeek.Tuesday, tuesdaySchedule);

        // Assert
        schedule.WeeklyHours.Should().HaveCount(2);
        schedule.WeeklyHours.Should().ContainKey(DayOfWeek.Monday);
        schedule.WeeklyHours.Should().ContainKey(DayOfWeek.Tuesday);
    }

    [Fact]
    public void Schedule_WithMultipleSpecialDates_ShouldContainAllDates()
    {
        // Arrange
        var schedule = new TestableSchedule(Guid.NewGuid());
        var christmas = new TestableSpecialDate(new DateOnly(2025, 12, 25), true, "Navidad", []);
        var newYear = new TestableSpecialDate(new DateOnly(2025, 1, 1), true, "Año Nuevo", []);

        // Act
        schedule.SetSpecialDate(christmas);
        schedule.SetSpecialDate(newYear);

        // Assert
        schedule.SpecialDates.Should().HaveCount(2);
        schedule.SpecialDates.Should().Contain(x => x.Date == new DateOnly(2025, 12, 25));
        schedule.SpecialDates.Should().Contain(x => x.Date == new DateOnly(2025, 1, 1));
    }

    #endregion
}
