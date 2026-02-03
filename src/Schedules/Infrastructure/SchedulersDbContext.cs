namespace Schedules.Infrastructure;

public class SchedulersDbContext(DbContextOptions<SchedulersDbContext> options, Guid tenantId) :
    DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    public IQueryable<T> Query<T>() where T : class, IEntity
    {
        return Set<T>().AsQueryable().AsNoTracking();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasQueryFilter(s => s.TenantId == tenantId);

            entity.Ignore(s => s.HasWeeklyHours);
            entity.Ignore(s => s.HasSpecialDates);
            entity.Ignore(s => s.IsFullyConfigured);

            // TODO: Implementar MapOf en Fudie.Firestore
            // entity.MapOf(s => s.WeeklyHours, daySchedule =>
            // {
            //     daySchedule.Ignore(ds => ds.TotalOpenHours);
            //
            //     daySchedule.ArrayOf(ds => ds.TimeSlots, timeSlot =>
            //     {
            //         timeSlot.Ignore(ts => ts.Duration);
            //     });
            // });

            entity.ArrayOf(s => s.SpecialDates, specialDate =>
            {
                specialDate.Ignore(sd => sd.TotalOpenHours);

                specialDate.ArrayOf(sd => sd.TimeSlots, timeSlot =>
                {
                    timeSlot.Ignore(ts => ts.Duration);
                });
            });
        });
    }
}
