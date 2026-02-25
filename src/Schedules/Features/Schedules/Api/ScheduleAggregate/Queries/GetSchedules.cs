namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Queries;

public class GetSchedules : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/schedules", Handler)
            .WithDescriptionCatalog("List all schedules");
    }

    public static Func<IService, Task<IResult>> Handler => async (service) =>
    {
        var response = await service.HandleAsync();
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<List<ScheduleResponse>> HandleAsync();
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<ScheduleResponse>> HandleAsync()
        {
            var schedules =  await query.Query<Schedule>()
                .OrderBy(s => s.Name)                
                .ToListAsync();

             return [.. schedules.Select(ScheduleResponse.Map)];
        }
    }
}
