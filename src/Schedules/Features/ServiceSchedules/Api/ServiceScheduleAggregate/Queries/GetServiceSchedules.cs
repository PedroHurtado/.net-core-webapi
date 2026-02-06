namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Queries;

public class GetServiceSchedules : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-schedules", Handler);
    }

    public static Func<IService, bool?, Task<IResult>> Handler => async (service, isActive) =>
    {
        var response = await service.HandleAsync(isActive);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<List<ServiceScheduleResponse>> HandleAsync(bool? isActive);
    }

    [Injectable]
    public class Service(IQuery query) : IService
    {
        public async Task<List<ServiceScheduleResponse>> HandleAsync(bool? isActive)
        {
            var queryable = query.Query<ServiceSchedule>();

            if (isActive.HasValue)
            {
                queryable = queryable.Where(s => s.IsActive == isActive.Value);
            }

             var serviceSchedules = await queryable
                .OrderBy(s => s.Name)
                .ToListAsync();

             return [.. serviceSchedules.Select(ServiceScheduleResponse.Map)];
        }
    }
}
