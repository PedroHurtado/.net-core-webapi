using FluentAssertions;
using Fudie;
using Schedule.Features.Schedule.Models;

namespace ScheDule.UnitTests;

public class ScheduleTests
{
    private readonly Guid _testId = Guid.NewGuid();
    private readonly Guid _testRestaurantId = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void Create_WithValidRestaurantId_ShouldReturnSuccess()
    {
        // Act
        var result = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(_testId);
        result.Value.RestaurantId.Should().Be(_testRestaurantId);
        result.Value.WeeklyHours.Should().HaveCount(7);
        result.Value.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyRestaurantId_ShouldReturnFailure()
    {
        // Act
        var result = Schedule.Features.Schedule.Models.Schedule.Create(_testId, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "RestaurantId" &&
            e.ErrorMessage == "RestaurantId es requerido");
    }

    [Fact]
    public void Create_ShouldInitializeAllDaysAsClosed()
    {
        // Act
        var result = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var daySchedule = result.Value!.WeeklyHours[day];
            daySchedule.IsClosed.Should().BeTrue();
            daySchedule.TimeSlots.Should().BeEmpty();
        }
    }

    #endregion

    #region SetWeeklyHours Tests

    [Fact]
    public void SetWeeklyHours_WithValidTimeSlot_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        var slots = new List<TimeSlot> { timeSlot };

        // Act
        var result = schedule.SetWeeklyHours(DayOfWeek.Monday, slots);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var mondaySchedule = schedule.WeeklyHours[DayOfWeek.Monday];
        mondaySchedule.IsClosed.Should().BeFalse();
        mondaySchedule.TimeSlots.Should().ContainSingle();
        mondaySchedule.TimeSlots.First().OpenTime.Should().Be(new TimeOnly(13, 0));
        mondaySchedule.TimeSlots.First().CloseTime.Should().Be(new TimeOnly(23, 0));
    }

    [Fact]
    public void SetWeeklyHours_WithInvalidTimeSlot_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(23, 0), new TimeOnly(13, 0));
        var slots = new List<TimeSlot> { timeSlot };

        // Act
        var result = schedule.SetWeeklyHours(DayOfWeek.Monday, slots);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "TimeSlot" &&
            e.ErrorMessage == "Hora de apertura debe ser antes del cierre");
    }

    [Fact]
    public void SetWeeklyHours_WithMultipleSlotsNoOverlap_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var slot1 = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slot2 = new TimeSlot(new TimeOnly(20, 0), new TimeOnly(23, 0));
        var slots = new List<TimeSlot> { slot1, slot2 };

        // Act
        var result = schedule.SetWeeklyHours(DayOfWeek.Tuesday, slots);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tuesdaySchedule = schedule.WeeklyHours[DayOfWeek.Tuesday];
        tuesdaySchedule.TimeSlots.Should().HaveCount(2);
    }

    [Fact]
    public void SetWeeklyHours_WithOverlappingSlots_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var slot1 = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slot2 = new TimeSlot(new TimeOnly(15, 0), new TimeOnly(18, 0));
        var slots = new List<TimeSlot> { slot1, slot2 };

        // Act
        var result = schedule.SetWeeklyHours(DayOfWeek.Wednesday, slots);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "TimeSlot" &&
            e.ErrorMessage == "Los horarios no pueden solaparse");
    }

    #endregion

    #region MarkDayAsClosed Tests

    [Fact]
    public void MarkDayAsClosed_ShouldSetDayAsClosedWithNoSlots()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Sunday, new List<TimeSlot> { timeSlot });

        // Act
        var result = schedule.MarkDayAsClosed(DayOfWeek.Sunday);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sundaySchedule = schedule.WeeklyHours[DayOfWeek.Sunday];
        sundaySchedule.IsClosed.Should().BeTrue();
        sundaySchedule.TimeSlots.Should().BeEmpty();
    }

    #endregion

    #region AddSpecialDate Tests

    [Fact]
    public void AddSpecialDate_WithValidClosedDate_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);

        // Act
        var result = schedule.AddSpecialDate(date, true, "Navidad", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.SpecialDates.Should().ContainSingle();
        var specialDate = schedule.SpecialDates.First();
        specialDate.Date.Should().Be(date);
        specialDate.IsClosed.Should().BeTrue();
        specialDate.Reason.Should().Be("Navidad");
        specialDate.TimeSlots.Should().BeEmpty();
    }

    [Fact]
    public void AddSpecialDate_WithValidOpenDate_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 2, 14);
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 59));
        var slots = new List<TimeSlot> { timeSlot };

        // Act
        var result = schedule.AddSpecialDate(date, false, "San Valentín", slots);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.SpecialDates.Should().ContainSingle();
        var specialDate = schedule.SpecialDates.First();
        specialDate.Date.Should().Be(date);
        specialDate.IsClosed.Should().BeFalse();
        specialDate.TimeSlots.Should().ContainSingle();
    }

    [Fact]
    public void AddSpecialDate_WithDuplicateDate_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);
        schedule.AddSpecialDate(date, true, "Navidad", null);

        // Act
        var result = schedule.AddSpecialDate(date, true, "Navidad otra vez", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(date) &&
            e.ErrorMessage == "Ya existe un horario especial para esta fecha");
    }

    [Fact]
    public void AddSpecialDate_ClosedWithSlots_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slots = new List<TimeSlot> { timeSlot };

        // Act
        var result = schedule.AddSpecialDate(date, true, "Navidad", slots);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(slots) &&
            e.ErrorMessage == "Un día cerrado no puede tener horarios");
    }

    [Fact]
    public void AddSpecialDate_OpenWithoutSlots_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 2, 14);

        // Act
        var result = schedule.AddSpecialDate(date, false, "San Valentín", null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "slots" &&
            e.ErrorMessage == "Un día abierto debe tener horarios");
    }

    #endregion

    #region RemoveSpecialDate Tests

    [Fact]
    public void RemoveSpecialDate_WithExistingDate_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);
        schedule.AddSpecialDate(date, true, "Navidad", null);

        // Act
        var result = schedule.RemoveSpecialDate(date);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSpecialDate_WithNonExistingDate_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);

        // Act
        var result = schedule.RemoveSpecialDate(date);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(date) &&
            e.ErrorMessage == "No existe un horario especial para esta fecha");
    }

    #endregion

    #region UpdateSpecialDate Tests

    [Fact]
    public void UpdateSpecialDate_WithExistingDate_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);
        schedule.AddSpecialDate(date, true, "Navidad", null);

        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slots = new List<TimeSlot> { timeSlot };

        // Act
        var result = schedule.UpdateSpecialDate(date, false, "Navidad - Abierto medio día", slots);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var specialDate = schedule.SpecialDates.First();
        specialDate.IsClosed.Should().BeFalse();
        specialDate.Reason.Should().Be("Navidad - Abierto medio día");
        specialDate.TimeSlots.Should().ContainSingle();
    }

    #endregion

    #region IsOpen Tests

    [Fact]
    public void IsOpen_WithinWeeklyHours_ShouldReturnTrue()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Tuesday, new List<TimeSlot> { timeSlot });

        var testDate = new DateTime(2025, 4, 15, 15, 0, 0); // Tuesday, 15:00

        // Act
        var isOpen = schedule.IsOpen(testDate);

        // Assert
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void IsOpen_OutsideWeeklyHours_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Tuesday, new List<TimeSlot> { timeSlot });

        var testDate = new DateTime(2025, 4, 15, 11, 0, 0); // Tuesday, 11:00

        // Act
        var isOpen = schedule.IsOpen(testDate);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpen_OnClosedDay_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        // Sunday está cerrado por defecto

        var testDate = new DateTime(2025, 4, 20, 15, 0, 0); // Sunday, 15:00

        // Act
        var isOpen = schedule.IsOpen(testDate);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpen_WithSpecialDateOpen_ShouldUseSpecialDate()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();

        // Configurar viernes normal: 13:00-23:00
        var weeklySlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Friday, new List<TimeSlot> { weeklySlot });

        // Configurar viernes especial (San Valentín): 13:00-23:59 (horario extendido)
        var specialDate = new DateOnly(2025, 2, 14); // Friday
        var specialSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 59));
        schedule.AddSpecialDate(specialDate, false, "San Valentín", new List<TimeSlot> { specialSlot });

        var testDate = new DateTime(2025, 2, 14, 23, 30, 0); // Friday, 23:30

        // Act
        var isOpen = schedule.IsOpen(testDate);

        // Assert
        isOpen.Should().BeTrue(); // Usa SpecialDate, no WeeklyHours
    }

    [Fact]
    public void IsOpen_WithSpecialDateClosed_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();

        // Configurar lunes normal: 13:00-23:00
        var weeklySlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Monday, new List<TimeSlot> { weeklySlot });

        // Configurar lunes especial (Año Nuevo) como cerrado
        var specialDate = new DateOnly(2025, 1, 1); // Monday
        schedule.AddSpecialDate(specialDate, true, "Año Nuevo", null);

        var testDate = new DateTime(2025, 1, 1, 14, 0, 0); // Monday, 14:00

        // Act
        var isOpen = schedule.IsOpen(testDate);

        // Assert
        isOpen.Should().BeFalse(); // SpecialDate dice cerrado
    }

    [Fact]
    public void IsOpen_BetweenMultipleSlots_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var slot1 = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(16, 0));
        var slot2 = new TimeSlot(new TimeOnly(20, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Friday, new List<TimeSlot> { slot1, slot2 });

        var testDate = new DateTime(2025, 4, 18, 17, 0, 0); // Friday, 17:00

        // Act
        var isOpen = schedule.IsOpen(testDate);

        // Assert
        isOpen.Should().BeFalse();
    }

    #endregion

    #region GetHoursFor Tests

    [Fact]
    public void GetHoursFor_WithNormalDay_ShouldReturnWeeklyHours()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Tuesday, new List<TimeSlot> { timeSlot });

        var date = new DateOnly(2025, 4, 15); // Tuesday

        // Act
        var hours = schedule.GetHoursFor(date);

        // Assert
        hours.Should().ContainSingle();
        hours.First().OpenTime.Should().Be(new TimeOnly(13, 0));
        hours.First().CloseTime.Should().Be(new TimeOnly(23, 0));
    }

    [Fact]
    public void GetHoursFor_WithClosedDay_ShouldReturnEmptyList()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        // Sunday está cerrado por defecto

        var date = new DateOnly(2025, 4, 20); // Sunday

        // Act
        var hours = schedule.GetHoursFor(date);

        // Assert
        hours.Should().BeEmpty();
    }

    [Fact]
    public void GetHoursFor_WithSpecialDate_ShouldReturnSpecialDateHours()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();

        // Configurar viernes normal
        var weeklySlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 0));
        schedule.SetWeeklyHours(DayOfWeek.Friday, new List<TimeSlot> { weeklySlot });

        // Configurar viernes especial (horario extendido hasta casi medianoche)
        var specialDate = new DateOnly(2025, 2, 14);
        var specialSlot = new TimeSlot(new TimeOnly(13, 0), new TimeOnly(23, 59));
        schedule.AddSpecialDate(specialDate, false, "San Valentín", new List<TimeSlot> { specialSlot });

        // Act
        var hours = schedule.GetHoursFor(specialDate);

        // Assert
        hours.Should().ContainSingle();
        hours.First().OpenTime.Should().Be(new TimeOnly(13, 0));
        hours.First().CloseTime.Should().Be(new TimeOnly(23, 59));
    }

    #endregion

    #region IsSpecialDate Tests

    [Fact]
    public void IsSpecialDate_WithExistingSpecialDate_ShouldReturnTrue()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);
        schedule.AddSpecialDate(date, true, "Navidad", null);

        // Act
        var isSpecial = schedule.IsSpecialDate(date);

        // Assert
        isSpecial.Should().BeTrue();
    }

    [Fact]
    public void IsSpecialDate_WithNonExistingSpecialDate_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var date = new DateOnly(2025, 12, 25);

        // Act
        var isSpecial = schedule.IsSpecialDate(date);

        // Assert
        isSpecial.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Schedule_With24HourDay_ShouldWork()
    {
        // Arrange
        var schedule = Schedule.Features.Schedule.Models.Schedule.Create(_testId, _testRestaurantId).ValueOrThrow();
        var timeSlot = new TimeSlot(new TimeOnly(0, 0), new TimeOnly(23, 59));
        schedule.SetWeeklyHours(DayOfWeek.Friday, new List<TimeSlot> { timeSlot });

        var testDate1 = new DateTime(2025, 4, 18, 3, 0, 0);
        var testDate2 = new DateTime(2025, 4, 18, 23, 45, 0);

        // Act
        var isOpen1 = schedule.IsOpen(testDate1);
        var isOpen2 = schedule.IsOpen(testDate2);

        // Assert
        isOpen1.Should().BeTrue();
        isOpen2.Should().BeTrue();
    }

    #endregion
}
