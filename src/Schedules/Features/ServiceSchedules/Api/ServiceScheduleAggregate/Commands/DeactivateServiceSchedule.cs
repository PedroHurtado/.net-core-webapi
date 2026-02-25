namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class DeactivateServiceSchedule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/service-schedules/{id}/deactivate", Handler)
            .WithDescriptionCatalog("Deactivate service schedule");
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
    public class Service(
        ServiceSchedule.Deactivate deactivateServiceSchedule,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ServiceScheduleResponse> HandleAsync(Guid id)
        {
            var serviceSchedule = await repository.Get(id);

            deactivateServiceSchedule.Execute(serviceSchedule);

            await unitOfWork.SaveChangesAsync();

            return ServiceScheduleResponse.Map(serviceSchedule);
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
