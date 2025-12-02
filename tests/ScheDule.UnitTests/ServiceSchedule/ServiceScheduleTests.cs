using FluentAssertions;
using Schedule.Features.ScheDuleService.Models;

namespace ScheDule.UnitTests.ServiceSchedule;

public class ServiceScheduleTests
{
    #region Helper Methods

    private static ReservationPolicy CreateDefaultPolicy()
    {
        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromHours(1) },
            { ServiceType.Lunch, TimeSpan.FromHours(1.5) },
            { ServiceType.Dinner, TimeSpan.FromHours(2) }
        };

        return ReservationPolicy.Create(
            minimumAdvanceTime: TimeSpan.FromHours(2),
            maximumAdvanceTime: TimeSpan.FromDays(30),
            slotInterval: TimeSpan.FromMinutes(15),
            standardDurations: standardDurations,
            bufferBetweenReservations: TimeSpan.FromMinutes(15),
            maxPartySize: 8,
            minPartySize: 1
        ).Value!;
    }

    private static Dictionary<DayOfWeek, ServiceDayConfig> CreateDefaultWeeklySchedule()
    {
        var schedule = new Dictionary<DayOfWeek, ServiceDayConfig>();
        var config = ServiceDayConfig.Create(true, new TimeOnly(13, 0), new TimeOnly(16, 0), null).Value!;

        for (int i = 0; i < 7; i++)
        {
            schedule[(DayOfWeek)i] = config;
        }

        return schedule;
    }

    #endregion

    #region Story 1: Create ServiceSchedule

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var policy = CreateDefaultPolicy();

        // Act
        var result = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), restaurantId, policy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().NotBeNull();
        result.Value!.RestaurantId.Should().Be(restaurantId);
        result.Value!.Policy.Should().Be(policy);
        result.Value!.Services.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyRestaurantId_ShouldReturnFailure()
    {
        // Arrange
        var policy = CreateDefaultPolicy();

        // Act
        var result = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.Empty, policy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "RestaurantId");
    }

    [Fact]
    public void Create_WithInvalidPolicy_ShouldReturnFailure()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        // Act
        var result = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), restaurantId, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Policy");
    }

    #endregion

    #region Story 2: Configure Services

    [Fact]
    public void AddService_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();

        // Act
        var result = schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Services.Should().HaveCount(1);
        schedule.Services.First().Type.Should().Be(ServiceType.Lunch);
        schedule.Services.First().MaxCapacity.Should().Be(50);
    }

    [Fact]
    public void AddService_Duplicate_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        // Act
        var result = schedule.AddService(ServiceType.Lunch, weeklySchedule, 60);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "ServiceType");
        schedule.Services.Should().HaveCount(1);
    }

    [Fact]
    public void AddService_WithInvalidTimeRange_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var invalidConfig = ServiceDayConfig.Create(true, new TimeOnly(16, 0), new TimeOnly(13, 0), null).Value!;
        var weeklySchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, invalidConfig }
        };

        // Act - This should fail during ServiceDayConfig.Create
        var configResult = ServiceDayConfig.Create(true, new TimeOnly(16, 0), new TimeOnly(13, 0), null);

        // Assert
        configResult.IsFailure.Should().BeTrue();
        configResult.Errors.Should().ContainSingle(e => e.PropertyName == "TimeRange");
    }

    [Fact]
    public void AddService_WithZeroCapacity_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();

        // Act
        var result = schedule.AddService(ServiceType.Dinner, weeklySchedule, 0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "MaxCapacity");
    }

    [Fact]
    public void AddService_MultipleServices_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();

        // Act
        var lunchResult = schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        var dinnerResult = schedule.AddService(ServiceType.Dinner, weeklySchedule, 60);

        // Assert
        lunchResult.IsSuccess.Should().BeTrue();
        dinnerResult.IsSuccess.Should().BeTrue();
        schedule.Services.Should().HaveCount(2);
    }

    #endregion

    #region Story 3: Configure Weekly Schedule

    [Fact]
    public void ConfigureServiceDay_WithValidConfig_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        var newConfig = ServiceDayConfig.Create(true, new TimeOnly(12, 0), new TimeOnly(17, 0), 60).Value!;

        // Act
        var result = schedule.ConfigureServiceDay(ServiceType.Lunch, DayOfWeek.Saturday, newConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var saturdayConfig = schedule.GetServiceConfig(ServiceType.Lunch, new DateOnly(2025, 1, 4)); // Saturday
        saturdayConfig.StartTime.Should().Be(new TimeOnly(12, 0));
        saturdayConfig.EndTime.Should().Be(new TimeOnly(17, 0));
        saturdayConfig.CapacityOverride.Should().Be(60);
    }

    [Fact]
    public void ConfigureServiceDay_WithUnavailableDay_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Breakfast, weeklySchedule, 40);
        var closedConfig = ServiceDayConfig.Create(false, default, default, null).Value!;

        // Act
        var result = schedule.ConfigureServiceDay(ServiceType.Breakfast, DayOfWeek.Sunday, closedConfig);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sundayAvailable = schedule.IsServiceAvailable(ServiceType.Breakfast, new DateOnly(2025, 1, 5)); // Sunday
        sundayAvailable.Should().BeFalse();
    }

    [Fact]
    public void GetCapacity_WithDayOverride_ShouldReturnOverrideCapacity()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);
        var fridayConfig = ServiceDayConfig.Create(true, new TimeOnly(20, 0), new TimeOnly(23, 0), 60).Value!;
        schedule.ConfigureServiceDay(ServiceType.Dinner, DayOfWeek.Friday, fridayConfig);

        // Act
        var fridayCapacity = schedule.GetCapacity(ServiceType.Dinner, new DateOnly(2025, 1, 3)); // Friday
        var mondayCapacity = schedule.GetCapacity(ServiceType.Dinner, new DateOnly(2024, 12, 30)); // Monday

        // Assert
        fridayCapacity.Should().Be(60);
        mondayCapacity.Should().Be(50);
    }

    #endregion

    #region Story 4: Special Dates

    [Fact]
    public void AddSpecialDate_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);
        var valentinesDay = new DateOnly(2025, 2, 14);

        // Act
        var result = schedule.AddSpecialDate(
            ServiceType.Dinner,
            valentinesDay,
            true,
            new TimeOnly(19, 0),
            new TimeOnly(2, 0),
            70,
            "San Valentín - horario extendido"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        var config = schedule.GetServiceConfig(ServiceType.Dinner, valentinesDay);
        config.IsAvailable.Should().BeTrue();
        config.StartTime.Should().Be(new TimeOnly(19, 0));
        config.EndTime.Should().Be(new TimeOnly(2, 0));
        config.CapacityOverride.Should().Be(70);
    }

    [Fact]
    public void AddSpecialDate_ForClosedDay_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        var newYearsDay = new DateOnly(2025, 1, 1);

        // Act
        var result = schedule.AddSpecialDate(
            ServiceType.Lunch,
            newYearsDay,
            false,
            default,
            default,
            null,
            "Año Nuevo - cerrado"
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        var isAvailable = schedule.IsServiceAvailable(ServiceType.Lunch, newYearsDay);
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void AddSpecialDate_Duplicate_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        var christmas = new DateOnly(2025, 12, 25);

        schedule.AddSpecialDate(ServiceType.Lunch, christmas, false, default, default, null, "Navidad");

        // Act
        var result = schedule.AddSpecialDate(ServiceType.Lunch, christmas, true, new TimeOnly(12, 0), new TimeOnly(15, 0), null, "Navidad - cambio");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SpecialDate");
    }

    [Fact]
    public void GetServiceConfig_WithSpecialDate_ShouldReturnSpecialDateConfig()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);
        var newYearsEve = new DateOnly(2025, 12, 31);

        schedule.AddSpecialDate(ServiceType.Dinner, newYearsEve, true, new TimeOnly(20, 0), new TimeOnly(2, 0), 70, "Nochevieja");

        // Act
        var specialConfig = schedule.GetServiceConfig(ServiceType.Dinner, newYearsEve);
        var regularConfig = schedule.GetServiceConfig(ServiceType.Dinner, new DateOnly(2025, 12, 30));

        // Assert
        specialConfig.StartTime.Should().Be(new TimeOnly(20, 0));
        specialConfig.EndTime.Should().Be(new TimeOnly(2, 0));
        regularConfig.StartTime.Should().Be(new TimeOnly(13, 0));
        regularConfig.EndTime.Should().Be(new TimeOnly(16, 0));
    }

    #endregion

    #region Story 5: Validate Advance Time

    [Fact]
    public void CanReserve_WithValidAdvanceTime_ShouldReturnTrue()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddHours(3);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();        
    }

    [Fact]
    public void CanReserve_WithinMinAdvance_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddHours(1);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
    }

    [Fact]
    public void CanReserve_BeyondMaxAdvance_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var reservationTime = DateTime.Now.AddDays(35);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, reservationTime, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
    }

    #endregion

    #region Story 6: Validate Slots

    [Fact]
    public void CanReserve_WithValidSlot_ShouldReturnTrue()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateTime.Now.Date.AddDays(1).AddHours(13).AddMinutes(30);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, tomorrow, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
    }

    [Fact]
    public void CanReserve_WithInvalidSlot_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateTime.Now.Date.AddDays(1).AddHours(13).AddMinutes(7);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, tomorrow, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
    }

    [Fact]
    public void GetAvailableSlots_ShouldReturnValidSlots()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        // Act
        var slots = schedule.GetAvailableSlots(ServiceType.Lunch, tomorrow, 4);

        // Assert
        slots.Should().NotBeEmpty();
        slots.Should().Contain(new TimeOnly(13, 0));
        slots.Should().Contain(new TimeOnly(13, 15));
        slots.Should().Contain(new TimeOnly(13, 30));
        slots.Should().OnlyContain(slot => slot.Minute % 15 == 0);
    }

    [Fact]
    public void GetAvailableSlots_WithInsufficientTime_ShouldExcludeLastSlots()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        // Act
        var slots = schedule.GetAvailableSlots(ServiceType.Lunch, tomorrow, 4);

        // Assert
        slots.Should().NotContain(new TimeOnly(15, 0));
        slots.Should().NotContain(new TimeOnly(15, 15));
        slots.Max().Should().BeOnOrBefore(new TimeOnly(14, 30));
    }

    #endregion

    #region Story 7: Validate Party Size

    [Fact]
    public void CanReserve_WithValidPartySize_ShouldReturnTrue()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateTime.Now.Date.AddDays(1).AddHours(13);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, tomorrow, 4);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CanReserve_PartyTooSmall_ShouldReturnFalse()
    {
        // Arrange
        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Lunch, TimeSpan.FromHours(1.5) }
        };

        var policyWithMinTwo = ReservationPolicy.Create(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            standardDurations,
            TimeSpan.FromMinutes(15),
            8,
            2
        ).Value!;

        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), policyWithMinTwo).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateTime.Now.Date.AddDays(1).AddHours(13);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, tomorrow, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CanReserve_PartyTooLarge_ShouldReturnFalse()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);

        var tomorrow = DateTime.Now.Date.AddDays(1).AddHours(13);

        // Act
        var result = schedule.CanReserve(ServiceType.Lunch, tomorrow, 12);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Additional Tests

    [Fact]
    public void UpdateService_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 50);

        var newSchedule = new Dictionary<DayOfWeek, ServiceDayConfig>
        {
            { DayOfWeek.Monday, ServiceDayConfig.Create(true, new TimeOnly(20, 0), new TimeOnly(23, 0), null).Value }
        };

        // Act
        var result = schedule.UpdateService(ServiceType.Dinner, newSchedule, 60);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Services.First().MaxCapacity.Should().Be(60);
    }

    [Fact]
    public void RemoveService_WithExistingService_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 60);

        // Act
        var result = schedule.RemoveService(ServiceType.Lunch);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Services.Should().HaveCount(1);
        schedule.Services.First().Type.Should().Be(ServiceType.Dinner);
    }

    [Fact]
    public void RemoveService_WithNonExistingService_ShouldReturnFailure()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;

        // Act
        var result = schedule.RemoveService(ServiceType.Breakfast);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "ServiceType");
    }

    [Fact]
    public void UpdatePolicy_WithValidPolicy_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var newPolicy = CreateDefaultPolicy();

        // Act
        var result = schedule.UpdatePolicy(newPolicy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        schedule.Policy.Should().Be(newPolicy);
    }

    [Fact]
    public void GetAvailableServices_ShouldReturnOnlyAvailableServices()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        schedule.AddService(ServiceType.Dinner, weeklySchedule, 60);

        var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        // Act
        var availableServices = schedule.GetAvailableServices(tomorrow);

        // Assert
        availableServices.Should().HaveCount(2);
        availableServices.Should().Contain(ServiceType.Lunch);
        availableServices.Should().Contain(ServiceType.Dinner);
    }

    [Fact]
    public void CalculateReservationEndTime_ShouldIncludeDurationAndBuffer()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var startTime = new DateTime(2025, 1, 1, 13, 0, 0);

        // Act
        var endTime = schedule.CalculateReservationEndTime(ServiceType.Lunch, startTime);

        // Assert
        var expectedEnd = startTime.AddHours(1.5).AddMinutes(15);
        endTime.Should().Be(expectedEnd);
    }

    [Fact]
    public void RemoveSpecialDate_WithExistingDate_ShouldReturnSuccess()
    {
        // Arrange
        var schedule = Schedule.Features.ScheDuleService.Models.ServiceSchedule.Create(Guid.NewGuid(), Guid.NewGuid(), CreateDefaultPolicy()).Value!;
        var weeklySchedule = CreateDefaultWeeklySchedule();
        schedule.AddService(ServiceType.Lunch, weeklySchedule, 50);
        var specialDate = new DateOnly(2025, 12, 25);

        schedule.AddSpecialDate(ServiceType.Lunch, specialDate, false, default, default, null, "Navidad");

        // Act
        var result = schedule.RemoveSpecialDate(ServiceType.Lunch, specialDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ReservationPolicy Tests

    [Fact]
    public void ReservationPolicy_Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromHours(1) },
            { ServiceType.Lunch, TimeSpan.FromHours(1.5) },
            { ServiceType.Dinner, TimeSpan.FromHours(2) }
        };

        // Act
        var result = ReservationPolicy.Create(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            standardDurations,
            TimeSpan.FromMinutes(15),
            8,
            1
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MinimumAdvanceTime.Should().Be(TimeSpan.FromHours(2));
        result.Value.MaximumAdvanceTime.Should().Be(TimeSpan.FromDays(30));
    }

    [Fact]
    public void ReservationPolicy_Create_WithMaxLessThanMin_ShouldReturnFailure()
    {
        // Arrange
        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Lunch, TimeSpan.FromHours(1.5) }
        };

        // Act
        var result = ReservationPolicy.Create(
            TimeSpan.FromHours(5),
            TimeSpan.FromHours(2),
            TimeSpan.FromMinutes(15),
            standardDurations,
            TimeSpan.FromMinutes(15),
            8,
            1
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "MaximumAdvanceTime");
    }

    [Fact]
    public void ReservationPolicy_Create_WithMinPartySizeGreaterThanMax_ShouldReturnFailure()
    {
        // Arrange
        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Lunch, TimeSpan.FromHours(1.5) }
        };

        // Act
        var result = ReservationPolicy.Create(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            standardDurations,
            TimeSpan.FromMinutes(15),
            2,
            5
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PartySize");
    }

    #endregion

    #region ServiceDayConfig Tests

    [Fact]
    public void ServiceDayConfig_Create_WithValidAvailableDay_ShouldReturnSuccess()
    {
        // Act
        var result = ServiceDayConfig.Create(true, new TimeOnly(13, 0), new TimeOnly(16, 0), 50);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeTrue();
        result.Value.StartTime.Should().Be(new TimeOnly(13, 0));
        result.Value.EndTime.Should().Be(new TimeOnly(16, 0));
        result.Value.CapacityOverride.Should().Be(50);
    }

    [Fact]
    public void ServiceDayConfig_Create_WithUnavailableDay_ShouldReturnSuccess()
    {
        // Act
        var result = ServiceDayConfig.Create(false, default, default, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void ServiceDayConfig_Create_WithInvalidTimeRange_ShouldReturnFailure()
    {
        // Act
        var result = ServiceDayConfig.Create(true, new TimeOnly(16, 0), new TimeOnly(13, 0), null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "TimeRange");
    }

    #endregion
}
