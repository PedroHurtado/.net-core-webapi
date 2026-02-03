namespace Schedules.UnitTests.Features.ServiceSchedules.Api.ServiceScheduleAggregate;

public class ServiceScheduleResponseTests
{
    #region ServiceScheduleResponse.Map

    [Fact]
    public void ServiceScheduleResponse_Map_MapsAllProperties()
    {
        var scheduleId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(15),
            8,
            1);

        var schedule = new TestableServiceSchedule(scheduleId);
        schedule.SetTenantId(tenantId);
        schedule.SetName("Summer Schedule");
        schedule.SetDescription("June to September schedule");
        schedule.SetIsActive(true);
        schedule.SetPolicy(policy);

        var response = ServiceScheduleResponse.Map(schedule);

        response.Id.Should().Be(scheduleId);
        response.Name.Should().Be("Summer Schedule");
        response.Description.Should().Be("June to September schedule");
        response.IsActive.Should().BeTrue();
        response.Policy.Should().NotBeNull();
        response.HasServices.Should().BeFalse();
        response.ServiceCount.Should().Be(0);
        response.AvailableServiceTypes.Should().BeEmpty();
        response.Services.Should().BeEmpty();
    }

    [Fact]
    public void ServiceScheduleResponse_Map_WithServices_MapsServices()
    {
        var schedule = new TestableServiceSchedule(Guid.NewGuid());
        schedule.SetTenantId(Guid.NewGuid());
        schedule.SetName("Test Schedule");
        schedule.SetDescription("Test Description");
        schedule.SetPolicy(ReservationPolicy.Default());

        var service = new TestableService(ServiceType.Lunch, 50);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = new TestableServiceDayConfig(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0));
        schedule.AddService(service);

        var response = ServiceScheduleResponse.Map(schedule);

        response.HasServices.Should().BeTrue();
        response.ServiceCount.Should().Be(1);
        response.AvailableServiceTypes.Should().HaveCount(1);
        response.AvailableServiceTypes.Should().Contain(ServiceType.Lunch);
        response.Services.Should().HaveCount(1);
        response.Services.First().Type.Should().Be(ServiceType.Lunch);
    }

    #endregion

    #region ReservationPolicyResponse.Map

    [Fact]
    public void ReservationPolicyResponse_Map_MapsAllProperties()
    {
        var standardDurations = new Dictionary<ServiceType, TimeSpan>
        {
            { ServiceType.Breakfast, TimeSpan.FromHours(1) },
            { ServiceType.Lunch, TimeSpan.FromHours(1.5) },
            { ServiceType.Dinner, TimeSpan.FromHours(2) }
        };

        var policy = new TestableReservationPolicy(
            TimeSpan.FromHours(2),
            TimeSpan.FromDays(30),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(15),
            8,
            1);
        policy.StandardDurationsInternal = standardDurations;

        var response = ReservationPolicyResponse.Map(policy);

        response.MinimumAdvanceTime.Should().Be(TimeSpan.FromHours(2));
        response.MaximumAdvanceTime.Should().Be(TimeSpan.FromDays(30));
        response.SlotInterval.Should().Be(TimeSpan.FromMinutes(15));
        response.BufferBetweenReservations.Should().Be(TimeSpan.FromMinutes(15));
        response.MaxPartySize.Should().Be(8);
        response.MinPartySize.Should().Be(1);
        response.SlotIntervalMinutes.Should().Be(15);
        response.MaxAdvanceDays.Should().Be(30);
        response.StandardDurations.Should().HaveCount(3);
        response.StandardDurations[ServiceType.Breakfast].Should().Be(TimeSpan.FromHours(1));
        response.StandardDurations[ServiceType.Lunch].Should().Be(TimeSpan.FromHours(1.5));
        response.StandardDurations[ServiceType.Dinner].Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void ReservationPolicyResponse_Map_WithEmptyStandardDurations_MapsEmptyDictionary()
    {
        var policy = ReservationPolicy.Default();

        var response = ReservationPolicyResponse.Map(policy);

        response.StandardDurations.Should().BeEmpty();
    }

    #endregion

    #region ServiceResponse.Map

    [Fact]
    public void ServiceResponse_Map_MapsAllProperties()
    {
        var service = new TestableService(ServiceType.Lunch, 50);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = new TestableServiceDayConfig(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0));
        service.WeeklyScheduleInternal[DayOfWeek.Friday] = new TestableServiceDayConfig(
            true,
            new TimeOnly(12, 0),
            new TimeOnly(17, 0),
            60);

        var response = ServiceResponse.Map(service);

        response.Type.Should().Be(ServiceType.Lunch);
        response.MaxCapacity.Should().Be(50);
        response.HasSpecialDates.Should().BeFalse();
        response.AvailableDaysCount.Should().Be(2);
        response.WeeklySchedule.Should().HaveCount(2);
        response.WeeklySchedule[DayOfWeek.Monday].Should().NotBeNull();
        response.WeeklySchedule[DayOfWeek.Friday].Should().NotBeNull();
        response.SpecialDates.Should().BeEmpty();
    }

    [Fact]
    public void ServiceResponse_Map_WithSpecialDates_MapsSpecialDates()
    {
        var service = new TestableService(ServiceType.Dinner, 40);
        service.WeeklyScheduleInternal[DayOfWeek.Saturday] = new TestableServiceDayConfig(
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 0));

        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(2, 0),
            70,
            "San Valentín");
        service.SpecialDatesInternal.Add(specialDate);

        var response = ServiceResponse.Map(service);

        response.HasSpecialDates.Should().BeTrue();
        response.SpecialDates.Should().HaveCount(1);
        response.SpecialDates.First().Date.Should().Be(new DateOnly(2025, 2, 14));
        response.SpecialDates.First().Reason.Should().Be("San Valentín");
    }

    [Fact]
    public void ServiceResponse_Map_WithUnavailableDays_CountsOnlyAvailableDays()
    {
        var service = new TestableService(ServiceType.Breakfast, 30);
        service.WeeklyScheduleInternal[DayOfWeek.Monday] = new TestableServiceDayConfig(
            true,
            new TimeOnly(8, 0),
            new TimeOnly(11, 0));
        service.WeeklyScheduleInternal[DayOfWeek.Sunday] = new TestableServiceDayConfig(
            false,
            null,
            null);

        var response = ServiceResponse.Map(service);

        response.AvailableDaysCount.Should().Be(1);
        response.WeeklySchedule.Should().HaveCount(2);
    }

    #endregion

    #region ServiceDayConfigResponse.Map

    [Fact]
    public void ServiceDayConfigResponse_Map_Available_MapsAllProperties()
    {
        var config = new TestableServiceDayConfig(
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0),
            60);

        var response = ServiceDayConfigResponse.Map(config);

        response.IsAvailable.Should().BeTrue();
        response.StartTime.Should().Be(new TimeOnly(13, 0));
        response.EndTime.Should().Be(new TimeOnly(16, 0));
        response.CapacityOverride.Should().Be(60);
        response.Duration.Should().Be(TimeSpan.FromHours(3));
    }

    [Fact]
    public void ServiceDayConfigResponse_Map_Unavailable_MapsNullValues()
    {
        var config = new TestableServiceDayConfig(false, null, null);

        var response = ServiceDayConfigResponse.Map(config);

        response.IsAvailable.Should().BeFalse();
        response.StartTime.Should().BeNull();
        response.EndTime.Should().BeNull();
        response.CapacityOverride.Should().BeNull();
        response.Duration.Should().BeNull();
    }

    [Fact]
    public void ServiceDayConfigResponse_Map_WithoutCapacityOverride_MapsNull()
    {
        var config = new TestableServiceDayConfig(
            true,
            new TimeOnly(20, 0),
            new TimeOnly(23, 0));

        var response = ServiceDayConfigResponse.Map(config);

        response.CapacityOverride.Should().BeNull();
    }

    #endregion

    #region ServiceSpecialDateResponse.Map

    [Fact]
    public void ServiceSpecialDateResponse_Map_Available_MapsAllProperties()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 2, 14),
            true,
            new TimeOnly(19, 0),
            new TimeOnly(2, 0),
            70,
            "San Valentín - horario extendido");

        var response = ServiceSpecialDateResponse.Map(specialDate);

        response.Date.Should().Be(new DateOnly(2025, 2, 14));
        response.IsAvailable.Should().BeTrue();
        response.StartTime.Should().Be(new TimeOnly(19, 0));
        response.EndTime.Should().Be(new TimeOnly(2, 0));
        response.CapacityOverride.Should().Be(70);
        response.Reason.Should().Be("San Valentín - horario extendido");
    }

    [Fact]
    public void ServiceSpecialDateResponse_Map_Unavailable_MapsNullValues()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 1, 1),
            false,
            null,
            null,
            null,
            "Año Nuevo");

        var response = ServiceSpecialDateResponse.Map(specialDate);

        response.Date.Should().Be(new DateOnly(2025, 1, 1));
        response.IsAvailable.Should().BeFalse();
        response.StartTime.Should().BeNull();
        response.EndTime.Should().BeNull();
        response.CapacityOverride.Should().BeNull();
        response.Reason.Should().Be("Año Nuevo");
    }

    [Fact]
    public void ServiceSpecialDateResponse_Map_WithoutReason_MapsNull()
    {
        var specialDate = new TestableServiceSpecialDate(
            new DateOnly(2025, 3, 15),
            true,
            new TimeOnly(13, 0),
            new TimeOnly(16, 0));

        var response = ServiceSpecialDateResponse.Map(specialDate);

        response.Reason.Should().BeNull();
    }

    #endregion
}