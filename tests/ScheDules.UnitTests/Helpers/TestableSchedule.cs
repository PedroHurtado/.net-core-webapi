namespace Schedules.UnitTests.Helpers;

public class TestableSchedule : Schedule
{
    public TestableSchedule(Guid id) : base(id) { }

    public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    public void SetName(string name) => Name = name;
    public void SetDescription(string? description) => Description = description;
    public void SetIsActive(bool isActive) => IsActive = isActive;

    public void AddWeeklyHours(DayOfWeek day, DaySchedule schedule) => _weeklyHours[day] = schedule;
    public void AddSpecialDate(SpecialDate specialDate) => _specialDates.Add(specialDate);
}
