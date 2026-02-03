namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Queries;

public class IsOpen : IFeatureModule
{
    public record Response(
        bool IsOpen,
        DateOnly Date,
        TimeOnly Time,
        bool IsSpecialDate,
        string? SpecialDateReason);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/schedules/{id}/is-open", Handler);
    }

    public static Func<IService, Guid, DateTime?, Task<IResult>> Handler => async (service, id, dateTime) =>
    {
        var response = await service.HandleAsync(id, dateTime);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<Response> HandleAsync(Guid id, DateTime? dateTime);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<Response> HandleAsync(Guid id, DateTime? dateTime)
        {
            var schedule = await repository.Get(id);

            var checkDateTime = dateTime ?? DateTime.Now;
            var date = DateOnly.FromDateTime(checkDateTime);
            var time = TimeOnly.FromDateTime(checkDateTime);

            var specialDate = schedule.SpecialDates.FirstOrDefault(sd => sd.Date == date);

            if (specialDate is not null)
            {
                return new Response(
                    IsOpen: specialDate.IsOpenAt(time),
                    Date: date,
                    Time: time,
                    IsSpecialDate: true,
                    SpecialDateReason: specialDate.Reason);
            }

            var dayOfWeek = checkDateTime.DayOfWeek;
            var isOpen = schedule.WeeklyHours.TryGetValue(dayOfWeek, out var daySchedule)
                && daySchedule.IsOpenAt(time);

            return new Response(
                IsOpen: isOpen,
                Date: date,
                Time: time,
                IsSpecialDate: false,
                SpecialDateReason: null);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<Schedule, Guid> { }
}
