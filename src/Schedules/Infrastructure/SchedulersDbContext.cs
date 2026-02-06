using Fudie.Firestore.EntityFrameworkCore.Extensions;

namespace Schedules.Infrastructure;

public class SchedulersDbContext(DbContextOptions<SchedulersDbContext> options, Guid tenantId) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ServiceSchedule> ServiceSchedules => Set<ServiceSchedule>();

    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasQueryFilter(s => s.TenantId == tenantId);

            entity.Ignore(s => s.HasWeeklyHours);
            entity.Ignore(s => s.HasSpecialDates);
            entity.Ignore(s => s.IsFullyConfigured);

            entity.MapOf(s => s.WeeklyHours, daySchedule =>
            {
                daySchedule.Ignore(ds => ds.TotalOpenHours);

                daySchedule.ArrayOf(ds => ds.TimeSlots, timeSlot =>
                {
                    timeSlot.Ignore(ts => ts.Duration);
                });
            });

            entity.ArrayOf(s => s.SpecialDates, specialDate =>
            {
                specialDate.Ignore(sd => sd.TotalOpenHours);

                specialDate.ArrayOf(sd => sd.TimeSlots, timeSlot =>
                {
                    timeSlot.Ignore(ts => ts.Duration);
                });
            });
        });

        modelBuilder.Entity<ServiceSchedule>(entity =>
        {
            entity.HasQueryFilter(s => s.TenantId == tenantId);
            
            entity.Ignore(s => s.HasServices);
            entity.Ignore(s => s.ServiceCount);
            entity.Ignore(s => s.AvailableServiceTypes);

            
            entity.ComplexProperty(s => s.Policy, policy =>
            {            
                policy.Ignore(p => p.SlotIntervalMinutes);
                policy.Ignore(p => p.MaxAdvanceDays);             
                policy.MapOf(p => p.StandardDurations);
            });

            
            entity.ArrayOf(s => s.Services, service =>
            {
            
                service.Ignore(srv => srv.HasSpecialDates);
                service.Ignore(srv => srv.AvailableDaysCount);
            
                service.MapOf(srv => srv.WeeklySchedule, dayConfig =>
                {
                     dayConfig.Ignore(dc => dc.Duration);
                });

                // ArrayOf: SpecialDates
                service.ArrayOf(srv => srv.SpecialDates);
            });
        });
    }
}
