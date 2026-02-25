namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class DeactivateSchedule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedules/{id}/deactivate", Handler)
            .WithDescriptionCatalog("Deactivate a schedule");
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
        Schedule.Deactivate deactivateSchedule,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ScheduleResponse> HandleAsync(Guid id)
        {
            var schedule = await repository.Get(id);

            deactivateSchedule.Execute(schedule);

            await unitOfWork.SaveChangesAsync();

            return ScheduleResponse.Map(schedule);
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
