namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class MarkDayAsClosed : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedules/{id}/weekly-hours/{dayOfWeek}/close", Handler)
            .WithDescriptionCatalog("Mark day as closed");
    }

    public static Func<IService, Guid, DayOfWeek, Task<IResult>> Handler => async (service, id, dayOfWeek) =>
    {
        await service.HandleAsync(id, dayOfWeek);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, DayOfWeek dayOfWeek);
    }

    [Injectable]
    public class Service(
        Schedule.MarkDayAsClosed markDayAsClosed,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, DayOfWeek dayOfWeek)
        {
            var schedule = await repository.Get(id);

            var command = new MarkDayAsClosedCommand(DayOfWeek: dayOfWeek);
            markDayAsClosed.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
