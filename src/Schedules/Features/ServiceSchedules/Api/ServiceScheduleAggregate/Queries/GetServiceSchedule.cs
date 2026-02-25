namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Queries;

public class GetServiceSchedule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-schedules/{id}", Handler)
            .WithDescriptionCatalog("Get service schedule by id");
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        var response = await service.HandleAsync(id);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<ServiceScheduleResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<ServiceScheduleResponse> HandleAsync(Guid id)
        {
            var serviceSchedule = await repository.Get(id);
            return ServiceScheduleResponse.Map(serviceSchedule);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<ServiceSchedule, Guid> { }
}
