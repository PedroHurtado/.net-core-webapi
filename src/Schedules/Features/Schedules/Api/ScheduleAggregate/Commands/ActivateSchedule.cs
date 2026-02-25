namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class ActivateSchedule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedules/{id}/activate", Handler)
            .WithDescriptionCatalog("Activate a schedule");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        var response = await service.HandleAsync(id);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<ScheduleResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        Schedule.Activate activateSchedule,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ScheduleResponse> HandleAsync(Guid id)
        {
            var schedule = await repository.Get(id);
            var currentActive = await repository.FindFirstByIsActiveTrue();

            var command = new ActivateScheduleCommand(CurrentActive: currentActive);
            activateSchedule.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();

            return ScheduleResponse.Map(schedule);
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid>
    {
        [Tracking]
        Task<Schedule?> FindFirstByIsActiveTrue();
    }
}
