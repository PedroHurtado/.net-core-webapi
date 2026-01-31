namespace Schedules.UnitTests.Helpers;

public class TestableServiceSchedule : ServiceSchedule
{
    public TestableServiceSchedule(Guid id) : base(id) { }

    public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    public void SetName(string name) => Name = name;
    public void SetDescription(string description) => Description = description;
    public void SetIsActive(bool isActive) => IsActive = isActive;
    public void SetPolicy(ReservationPolicy policy) => Policy = policy;

    public HashSet<Service> ServicesInternal
    {
        get => _services;
        set => _services = value;
    }

    public void AddService(Service service)
    {
        _services.Add(service);
    }
}
