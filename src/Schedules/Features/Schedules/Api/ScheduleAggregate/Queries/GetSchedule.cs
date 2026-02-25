namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Queries;

public class GetSchedule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/schedules/{id}", Handler)
            .WithDescriptionCatalog("Get schedule by id");
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
    public class Service(IRepository repository) : IService
    {
        public async Task<ScheduleResponse> HandleAsync(Guid id)
        {
            var schedule = await repository.Get(id);
            return ScheduleResponse.Map(schedule);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<Schedule, Guid> { }
}
