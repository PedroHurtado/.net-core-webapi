namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class SetWeeklyHours : IFeatureModule
{
    public record Request(
        bool IsClosed,
        SetTimeSlotRequest[] TimeSlots);

    public record SetTimeSlotRequest(
        TimeOnly OpenTime,
        TimeOnly CloseTime);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/schedules/{id}/weekly-hours/{dayOfWeek}", Handler)
            .WithDescriptionCatalog("Set weekly hours for a day");
    }

    public static Func<IService, Guid, DayOfWeek, Request, Task<IResult>> Handler => async (service, id, dayOfWeek, request) =>
    {
        await service.HandleAsync(id, dayOfWeek, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, DayOfWeek dayOfWeek, Request request);
    }

    [Injectable]
    public class Service(
        Schedule.SetWeeklyHours setWeeklyHours,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, DayOfWeek dayOfWeek, Request request)
        {
            var schedule = await repository.Get(id);

            var command = new SetWeeklyHoursCommand(
                DayOfWeek: dayOfWeek,
                IsClosed: request.IsClosed,
                TimeSlots: request.TimeSlots
                    .Select(ts => new CreateTimeSlotCommand(ts.OpenTime, ts.CloseTime))
                    .ToArray());

            setWeeklyHours.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
