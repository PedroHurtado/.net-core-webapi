namespace Schedules.UnitTests.Features.ServiceSchedules.Domain.ServiceScheduleAggregateTests;

public class ServiceScheduleTests
{
    #region Properties

    [Fact]
    public void ServiceSchedule_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var name = "Main Schedule";
        var description = "Main restaurant schedule";
        var policy = ReservationPolicy.Default();

        // Act
        var schedule = new TestableServiceSchedule(id);
        schedule.SetTenantId(tenantId);
        schedule.SetName(name);
        schedule.SetDescription(description);
        schedule.SetIsActive(true);
        schedule.SetPolicy(policy);

        // Assert
        schedule.Id.Should().Be(id);
        schedule.TenantId.Should().Be(tenantId);
        schedule.Name.Should().Be(name);
        schedule.Description.Should().Be(description);
        schedule.IsActive.Should().BeTrue();
        schedule.Policy.Should().Be(policy);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ServiceSchedule_IsActive_ShouldSetCorrectly(bool value)
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());

        // Act
        schedule.SetIsActive(value);

        // Assert
        schedule.IsActive.Should().Be(value);
    }

    #endregion

    #region Collections

    [Fact]
    public void Services_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);

        // Act
        schedule.AddService(service);

        // Assert
        schedule.Services.Should().BeAssignableTo<IReadOnlyCollection<Service>>();
        schedule.Services.Should().ContainSingle();
    }

    [Fact]
    public void ServiceSchedule_WithMultipleServices_ShouldContainAllServices()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var breakfast = new TestableService(ServiceType.Breakfast, 30);
        var lunch = new TestableService(ServiceType.Lunch, 50);
        var dinner = new TestableService(ServiceType.Dinner, 40);

        // Act
        schedule.AddService(breakfast);
        schedule.AddService(lunch);
        schedule.AddService(dinner);

        // Assert
        schedule.Services.Should().HaveCount(3);
        schedule.Services.Should().Contain(x => x.Type == ServiceType.Breakfast);
        schedule.Services.Should().Contain(x => x.Type == ServiceType.Lunch);
        schedule.Services.Should().Contain(x => x.Type == ServiceType.Dinner);
    }

    #endregion

    #region Computed Properties

    [Fact]
    public void HasServices_WithServices_ShouldReturnTrue()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);

        // Act
        schedule.AddService(service);

        // Assert
        schedule.HasServices.Should().BeTrue();
    }

    [Fact]
    public void HasServices_WithoutServices_ShouldReturnFalse()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());

        // Assert
        schedule.HasServices.Should().BeFalse();
    }

    [Fact]
    public void ServiceCount_WithMultipleServices_ShouldReturnCorrectCount()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var breakfast = new TestableService(ServiceType.Breakfast, 30);
        var lunch = new TestableService(ServiceType.Lunch, 50);

        // Act
        schedule.AddService(breakfast);
        schedule.AddService(lunch);

        // Assert
        schedule.ServiceCount.Should().Be(2);
    }

    [Fact]
    public void AvailableServiceTypes_WithMultipleServices_ShouldReturnAllTypes()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var breakfast = new TestableService(ServiceType.Breakfast, 30);
        var dinner = new TestableService(ServiceType.Dinner, 40);

        // Act
        schedule.AddService(breakfast);
        schedule.AddService(dinner);

        // Assert
        schedule.AvailableServiceTypes.Should().HaveCount(2);
        schedule.AvailableServiceTypes.Should().Contain(ServiceType.Breakfast);
        schedule.AvailableServiceTypes.Should().Contain(ServiceType.Dinner);
    }

    #endregion

    #region Business Methods - GetServiceConfigForDate

    [Fact]
    public void GetServiceConfigForDate_WhenServiceNotExists_ShouldReturnNull()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var date = new DateOnly(2025, 1, 15);

        // Act
        var config = schedule.GetServiceConfigForDate(ServiceType.Lunch, date);

        // Assert
        config.Should().BeNull();
    }

    [Fact]
    public void GetServiceConfigForDate_WithWeeklySchedule_ShouldReturnWeeklyConfig()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;
        schedule.AddService(service);

        var monday = new DateOnly(2025, 1, 13); // Monday

        // Act
        var config = schedule.GetServiceConfigForDate(ServiceType.Lunch, monday);

        // Assert
        config.Should().NotBeNull();
        config!.IsAvailable.Should().BeTrue();
        config.StartTime.Should().Be(new TimeOnly(12, 0));
        config.EndTime.Should().Be(new TimeOnly(15, 0));
    }

    [Fact]
    public void GetServiceConfigForDate_WithSpecialDate_ShouldReturnSpecialDateConfig()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;

        var specialDate = new DateOnly(2025, 1, 13); // Monday
        var specialDateConfig = new TestableServiceSpecialDate(
            specialDate,
            true,
            new TimeOnly(11, 0),
            new TimeOnly(16, 0),
            100,
            "Extended hours");
        service.SpecialDatesInternal.Add(specialDateConfig);
        
        schedule.AddService(service);

        // Act
        var config = schedule.GetServiceConfigForDate(ServiceType.Lunch, specialDate);

        // Assert
        config.Should().NotBeNull();
        config!.IsAvailable.Should().BeTrue();
        config.StartTime.Should().Be(new TimeOnly(11, 0));
        config.EndTime.Should().Be(new TimeOnly(16, 0));
        config.CapacityOverride.Should().Be(100);
    }

    [Fact]
    public void GetServiceConfigForDate_WhenDayNotInWeeklySchedule_ShouldReturnNull()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;
        schedule.AddService(service);

        var tuesday = new DateOnly(2025, 1, 14); // Tuesday

        // Act
        var config = schedule.GetServiceConfigForDate(ServiceType.Lunch, tuesday);

        // Assert
        config.Should().BeNull();
    }

    #endregion

    #region Business Methods - IsServiceAvailableOn

    [Fact]
    public void IsServiceAvailableOn_WhenServiceNotExists_ShouldReturnFalse()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var date = new DateOnly(2025, 1, 15);

        // Act
        var isAvailable = schedule.IsServiceAvailableOn(ServiceType.Lunch, date);

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsServiceAvailableOn_WhenServiceAvailable_ShouldReturnTrue()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;
        schedule.AddService(service);

        var monday = new DateOnly(2025, 1, 13); // Monday

        // Act
        var isAvailable = schedule.IsServiceAvailableOn(ServiceType.Lunch, monday);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsServiceAvailableOn_WhenServiceNotAvailable_ShouldReturnFalse()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        var weeklyConfig = new TestableServiceDayConfig(false, null, null, null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;
        schedule.AddService(service);

        var monday = new DateOnly(2025, 1, 13); // Monday

        // Act
        var isAvailable = schedule.IsServiceAvailableOn(ServiceType.Lunch, monday);

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Business Methods - GetServiceCapacityFor

    [Fact]
    public void GetServiceCapacityFor_WhenServiceNotExists_ShouldReturnNull()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var date = new DateOnly(2025, 1, 15);

        // Act
        var capacity = schedule.GetServiceCapacityFor(ServiceType.Lunch, date);

        // Assert
        capacity.Should().BeNull();
    }

    [Fact]
    public void GetServiceCapacityFor_WithoutCapacityOverride_ShouldReturnMaxCapacity()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;
        schedule.AddService(service);

        var monday = new DateOnly(2025, 1, 13); // Monday

        // Act
        var capacity = schedule.GetServiceCapacityFor(ServiceType.Lunch, monday);

        // Assert
        capacity.Should().Be(50);
    }

    [Fact]
    public void GetServiceCapacityFor_WithCapacityOverride_ShouldReturnOverride()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), 75);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;
        schedule.AddService(service);

        var monday = new DateOnly(2025, 1, 13); // Monday

        // Act
        var capacity = schedule.GetServiceCapacityFor(ServiceType.Lunch, monday);

        // Assert
        capacity.Should().Be(75);
    }

    [Fact]
    public void GetServiceCapacityFor_WithSpecialDateOverride_ShouldReturnSpecialDateCapacity()
    {
        // Arrange
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        var service = new TestableService(ServiceType.Lunch, 50);
        
        var weeklyConfig = new TestableServiceDayConfig(true, new TimeOnly(12, 0), new TimeOnly(15, 0), null);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = weeklyConfig;

        var specialDate = new DateOnly(2025, 1, 13); // Monday
        var specialDateConfig = new TestableServiceSpecialDate(
            specialDate,
            true,
            new TimeOnly(11, 0),
            new TimeOnly(16, 0),
            100,
            "Extended capacity");
        service.SpecialDatesInternal.Add(specialDateConfig);
        
        schedule.AddService(service);

        // Act
        var capacity = schedule.GetServiceCapacityFor(ServiceType.Lunch, specialDate);

        // Assert
        capacity.Should().Be(100);
    }

    #endregion
}
