namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class UpdateSchedule : IFeatureModule
{
    public record Request(
        string Name,
        string? Description);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/schedules/{id}", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        await service.HandleAsync(id, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Schedule.Update updateSchedule,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var schedule = await repository.Get(id);

            var command = new UpdateScheduleCommand(
                Name: request.Name,
                Description: request.Description);

            updateSchedule.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
