using FluentAssertions;
using Schedule.Features.ServiceSchedule.Models;

namespace ScheDule.UnitTests;

public class ServiceScheduleTests
{
    private readonly Guid _testId = Guid.NewGuid();
    private readonly Guid _restaurantId = Guid.NewGuid();
    private ReservationPolicy _defaultPolicy;

    public ServiceScheduleTests()
    {
        _defaultPolicy = ReservationPolicy.CreateDefault();
    }

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Act
        var result = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(_testId);
        result.Value.RestaurantId.Should().Be(_restaurantId);
        result.Value.Policy.Should().Be(_defaultPolicy);
        result.Value.Services.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyRestaurantId_ShouldReturnFailure()
    {
        // Act
        var result = ServiceSchedule.Create(_testId, Guid.Empty, _defaultPolicy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "RestaurantId" &&
            e.ErrorMessage == "RestaurantId es requerido");
    }

    [Fact]
    public void Create_WithInvalidPolicy_ShouldReturnFailure()
    {
        // Arrange - MinAdvance > MaxAdvance
        var invalidPolicy = new ReservationPolicy(
            minimumAdvanceTime: TimeSpan.FromHours(5),
            maximumAdvanceTime: TimeSpan.FromHours(2),
            slotInterval: TimeSpan.FromMinutes(15),
            standardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromHours(1.5) }
            },
            bufferBetweenReservations: TimeSpan.FromMinutes(15),
            maxPartySize: 8,
            minPartySize: 1
        );

        // Act
        var result = ServiceSchedule.Create(_testId, _restaurantId, invalidPolicy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Tiempo máximo debe ser mayor que tiempo mínimo"));
    }

    #endregion

    #region AddService Tests

    [Fact]
    public void AddService_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Tuesday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };

        // Act
        var result = schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Services.Should().ContainSingle();
        schedule.Services.First().Type.Should().Be(ServiceType.Lunch);
        schedule.Services.First().MaxCapacity.Should().Be(50);
    }

    [Fact]
    public void AddService_Duplicate_ShouldReturnFailure()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Act
        var result = schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "ServiceType" &&
            e.ErrorMessage.Contains("Ya existe un servicio de tipo"));
    }

    [Fact]
    public void AddService_WithInvalidTimeRange_ShouldReturnFailure()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(16, 0), new TimeOnly(13, 0), null) } // Invalid: start > end
        };

        // Act
        var result = schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "WeeklySchedule" &&
            e.ErrorMessage.Contains("Hora de inicio debe ser antes de hora de fin"));
    }

    [Fact]
    public void AddService_WithZeroCapacity_ShouldReturnFailure()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };

        // Act
        var result = schedule.AddService(ServiceType.Lunch, weeklySchedule, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "MaxCapacity" &&
            e.ErrorMessage == "Capacidad debe ser mayor que 0");
    }

    [Fact]
    public void AddService_MultipleServices_ShouldAddAll()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var lunchSchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        var dinnerSchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
        };

        // Act
        schedule.AddService(ServiceType.Lunch, lunchSchedule, 50);
        schedule.AddService(ServiceType.Dinner, dinnerSchedule, 40);

        // Assert
        schedule.Services.Should().HaveCount(2);
        schedule.Services.Should().Contain(s => s.Type == ServiceType.Lunch);
        schedule.Services.Should().Contain(s => s.Type == ServiceType.Dinner);
    }

    #endregion

    #region UpdateService Tests

    [Fact]
    public void UpdateService_WithValidData_ShouldUpdateService()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var initialSchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, initialSchedule, 50);

        var updatedSchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(17, 0), null) }
        };

        // Act
        var result = schedule.UpdateService(ServiceType.Lunch, updatedSchedule, 60);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var service = schedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.MaxCapacity.Should().Be(60);
        service.WeeklySchedule[DayOfWeek.Monday].StartTime.Should().Be(new TimeOnly(12, 0));
    }

    [Fact]
    public void UpdateService_NonExistingService_ShouldReturnFailure()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };

        // Act
        var result = schedule.UpdateService(ServiceType.Lunch, weeklySchedule, 50);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "ServiceType" &&
            e.ErrorMessage.Contains("No existe un servicio de tipo"));
    }

    #endregion

    #region RemoveService Tests

    [Fact]
    public void RemoveService_ExistingService_ShouldRemoveService()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Act
        var result = schedule.RemoveService(ServiceType.Lunch);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Services.Should().BeEmpty();
    }

    [Fact]
    public void RemoveService_NonExistingService_ShouldReturnFailure()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;

        // Act
        var result = schedule.RemoveService(ServiceType.Lunch);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "ServiceType" &&
            e.ErrorMessage.Contains("No existe un servicio de tipo"));
    }

    #endregion

    #region ConfigureServiceDay Tests

    [Fact]
    public void ConfigureServiceDay_WithValidData_ShouldConfigureDay()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var newConfig = new ServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), 60);

        // Act
        var result = schedule.ConfigureServiceDay(ServiceType.Lunch, DayOfWeek.Tuesday, newConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var service = schedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.WeeklySchedule[DayOfWeek.Tuesday].Should().Be(newConfig);
    }

    [Fact]
    public void ConfigureServiceDay_DisableService_ShouldSetUnavailable()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Sunday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var disabledConfig = new ServiceDayConfig(false, TimeOnly.MinValue, TimeOnly.MinValue, null);

        // Act
        var result = schedule.ConfigureServiceDay(ServiceType.Lunch, DayOfWeek.Sunday, disabledConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var service = schedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.WeeklySchedule[DayOfWeek.Sunday].IsAvailable.Should().BeFalse();
    }

    #endregion

    #region AddSpecialDate Tests

    [Fact]
    public void AddSpecialDate_WithValidData_ShouldAddSpecialDate()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
        };
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);

        var valentinesDay = new DateOnly(2025, 2, 14);

        // Act
        var result = schedule.AddSpecialDate(
            ServiceType.Dinner,
            valentinesDay,
            isAvailable: true,
            startTime: new TimeOnly(19, 0),
            endTime: new TimeOnly(2, 0),
            capacityOverride: 60,
            reason: "San Valentín - horario extendido"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        var service = schedule.Services.First(s => s.Type == ServiceType.Dinner);
        service.SpecialDates.Should().ContainSingle();
        service.SpecialDates.First().Date.Should().Be(valentinesDay);
        service.SpecialDates.First().CapacityOverride.Should().Be(60);
    }

    [Fact]
    public void AddSpecialDate_ClosedDay_ShouldSetUnavailable()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var newYear = new DateOnly(2025, 1, 1);

        // Act
        var result = schedule.AddSpecialDate(
            ServiceType.Lunch,
            newYear,
            isAvailable: false,
            startTime: null,
            endTime: null,
            capacityOverride: null,
            reason: "Año Nuevo - cerrado"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        var service = schedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.SpecialDates.First().IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void AddSpecialDate_DuplicateDate_ShouldReturnFailure()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var christmas = new DateOnly(2025, 12, 25);
        schedule.AddSpecialDate(ServiceType.Lunch, christmas, false, null, null, null, "Navidad");

        // Act
        var result = schedule.AddSpecialDate(ServiceType.Lunch, christmas, false, null, null, null, "Navidad");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Date" &&
            e.ErrorMessage == "Ya existe horario especial para esta fecha");
    }

    #endregion

    #region RemoveSpecialDate Tests

    [Fact]
    public void RemoveSpecialDate_ExistingDate_ShouldRemoveDate()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var specialDate = new DateOnly(2025, 12, 25);
        schedule.AddSpecialDate(ServiceType.Lunch, specialDate, false, null, null, null, "Navidad");

        // Act
        var result = schedule.RemoveSpecialDate(ServiceType.Lunch, specialDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var service = schedule.Services.First(s => s.Type == ServiceType.Lunch);
        service.SpecialDates.Should().BeEmpty();
    }

    #endregion

    #region UpdatePolicy Tests

    [Fact]
    public void UpdatePolicy_WithValidPolicy_ShouldUpdatePolicy()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var newPolicy = new ReservationPolicy(
            minimumAdvanceTime: TimeSpan.FromHours(4),
            maximumAdvanceTime: TimeSpan.FromDays(60),
            slotInterval: TimeSpan.FromMinutes(30),
            standardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Breakfast, TimeSpan.FromHours(1) },
                { ServiceType.Lunch, TimeSpan.FromHours(2) },
                { ServiceType.Dinner, TimeSpan.FromHours(2.5) }
            },
            bufferBetweenReservations: TimeSpan.FromMinutes(20),
            maxPartySize: 10,
            minPartySize: 2
        );

        // Act
        var result = schedule.UpdatePolicy(newPolicy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Policy.Should().Be(newPolicy);
        schedule.Policy.MinimumAdvanceTime.Should().Be(TimeSpan.FromHours(4));
    }

    #endregion

    #region GetAvailableServices Tests

    [Fact]
    public void GetAvailableServices_WithRegularSchedule_ShouldReturnAvailableServices()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Sunday, new ServiceDayConfig(false, TimeOnly.MinValue, TimeOnly.MinValue, null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var monday = new DateOnly(2025, 1, 6); // Monday
        var sunday = new DateOnly(2025, 1, 5); // Sunday

        // Act
        var mondayServices = schedule.GetAvailableServices(monday);
        var sundayServices = schedule.GetAvailableServices(sunday);

        // Assert
        mondayServices.Should().Contain(ServiceType.Lunch);
        sundayServices.Should().NotContain(ServiceType.Lunch);
    }

    [Fact]
    public void GetAvailableServices_WithSpecialDate_ShouldUseSpecialDate()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
        };
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);

        var valentinesDay = new DateOnly(2025, 2, 14); // Friday
        schedule.AddSpecialDate(ServiceType.Dinner, valentinesDay, true, new TimeOnly(19, 0), new TimeOnly(2, 0), 60, "San Valentín");

        // Act
        var services = schedule.GetAvailableServices(valentinesDay);

        // Assert
        services.Should().Contain(ServiceType.Dinner);
    }

    #endregion

    #region CanReserve Tests

    [Fact]
    public void CanReserve_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Tuesday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Wednesday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Thursday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Friday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Saturday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Sunday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Use a date 5 days in the future at 13:00 (well within the 30-day max advance and beyond the 2-hour min)
        var futureDate = DateTime.Now.AddDays(5).Date.AddHours(13);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, futureDate, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void CanReserve_WithinMinAdvance_ShouldReturnFalse()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddHours(1); // 1 hour from now (MinAdvance = 2 hours)

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 4);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "AdvanceTime" &&
            e.ErrorMessage.Contains("al menos"));
    }

    [Fact]
    public void CanReserve_BeyondMaxAdvance_ShouldReturnFalse()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddDays(40); // 40 days from now (MaxAdvance = 30 days)

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 4);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "AdvanceTime" &&
            e.ErrorMessage.Contains("hasta"));
    }

    [Fact]
    public void CanReserve_InvalidSlot_ShouldReturnFalse()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddDays(1).Date.AddHours(13).AddMinutes(7); // 13:07 (not a 15-min slot)

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 4);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "SlotInterval" &&
            e.ErrorMessage.Contains("cada"));
    }

    [Fact]
    public void CanReserve_PartyTooLarge_ShouldReturnFalse()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddDays(1).Date.AddHours(13);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 12); // MaxPartySize = 8

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "PartySize" &&
            e.ErrorMessage.Contains("Máximo"));
    }

    [Fact]
    public void CanReserve_PartyTooSmall_ShouldReturnFalse()
    {
        // Arrange
        var policy = new ReservationPolicy(
            minimumAdvanceTime: TimeSpan.FromHours(2),
            maximumAdvanceTime: TimeSpan.FromDays(30),
            slotInterval: TimeSpan.FromMinutes(15),
            standardDurations: new Dictionary<ServiceType, TimeSpan>
            {
                { ServiceType.Lunch, TimeSpan.FromHours(1.5) }
            },
            bufferBetweenReservations: TimeSpan.FromMinutes(15),
            maxPartySize: 8,
            minPartySize: 2 // Minimum 2 people
        );

        var schedule = ServiceSchedule.Create(_testId, _restaurantId, policy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddDays(1).Date.AddHours(13);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "PartySize" &&
            e.ErrorMessage.Contains("Mínimo"));
    }

    #endregion

    #region GetAvailableSlots Tests

    [Fact]
    public void GetAvailableSlots_ShouldReturnSlotsWithinServiceHours()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var monday = new DateOnly(2025, 1, 6);

        // Act
        var slots = schedule.GetAvailableSlots(ServiceType.Lunch, monday, 4);

        // Assert
        // Lunch duration = 1.5h, so last slot should be 14:30 (ends at 16:00)
        // Slots: 13:00, 13:15, 13:30, 13:45, 14:00, 14:15, 14:30
        slots.Should().Contain(new TimeOnly(13, 0));
        slots.Should().Contain(new TimeOnly(14, 30));
        slots.Should().NotContain(new TimeOnly(14, 45)); // Would end at 16:15
    }

    [Fact]
    public void GetAvailableSlots_WithSpecialDate_ShouldUseSpecialDate()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Friday, new ServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
        };
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);

        var valentinesDay = new DateOnly(2025, 2, 14); // Friday
        schedule.AddSpecialDate(ServiceType.Dinner, valentinesDay, true, new TimeOnly(19, 0), new TimeOnly(2, 0), 60, "San Valentín");

        // Act
        var slots = schedule.GetAvailableSlots(ServiceType.Dinner, valentinesDay, 2);

        // Assert
        // Should start at 19:00 (special date), not 20:00 (regular)
        slots.Should().Contain(new TimeOnly(19, 0));
        slots.Should().NotBeEmpty();
    }

    #endregion

    #region CalculateReservationEndTime Tests

    [Fact]
    public void CalculateReservationEndTime_ShouldIncludeDurationAndBuffer()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var startTime = new DateTime(2025, 1, 6, 13, 0, 0);

        // Act
        var endTime = schedule.CalculateReservationEndTime(ServiceType.Lunch, startTime);

        // Assert
        // Lunch duration = 1.5h, buffer = 15min
        // End time = 13:00 + 1:30 + 0:15 = 14:45
        endTime.Should().Be(new DateTime(2025, 1, 6, 14, 45, 0));
    }

    #endregion

    #region GetCapacity Tests

    [Fact]
    public void GetCapacity_WithoutOverride_ShouldReturnMaxCapacity()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) }
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var monday = new DateOnly(2025, 1, 6);

        // Act
        var capacity = schedule.GetCapacity(ServiceType.Lunch, monday);

        // Assert
        capacity.Should().Be(50);
    }

    [Fact]
    public void GetCapacity_WithDayOverride_ShouldReturnOverride()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null) },
            { DayOfWeek.Friday, new ServiceDayConfig(true, new TimeOnly(13, 0), new TimeOnly(16, 0), 60) } // Override
        };
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var friday = new DateOnly(2025, 1, 10);

        // Act
        var capacity = schedule.GetCapacity(ServiceType.Lunch, friday);

        // Assert
        capacity.Should().Be(60);
    }

    [Fact]
    public void GetCapacity_WithSpecialDateOverride_ShouldReturnSpecialDateCapacity()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Friday, new ServiceDayConfig(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null) }
        };
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);

        var newYearsEve = new DateOnly(2025, 12, 31);
        schedule.AddSpecialDate(ServiceType.Dinner, newYearsEve, true, new TimeOnly(19, 0), new TimeOnly(2, 0), 70, "Nochevieja");

        // Act
        var capacity = schedule.GetCapacity(ServiceType.Dinner, newYearsEve);

        // Assert
        capacity.Should().Be(70);
    }

    #endregion

    #region Services Collection Tests

    [Fact]
    public void Services_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var schedule = ServiceSchedule.Create(_testId, _restaurantId, _defaultPolicy).Value!;

        // Act
        var services = schedule.Services;

        // Assert
        services.Should().BeAssignableTo<IReadOnlyCollection<Service>>();
    }

    #endregion
}
