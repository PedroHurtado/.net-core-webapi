namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class RemoveService : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/service-schedules/{id}/services/{type}", Handler)
            .WithDescriptionCatalog("Remove service");
    }

    public static Func<IService, Guid, ServiceType, Task<IResult>> Handler => async (service, id, type) =>
    {
        await service.HandleAsync(id, type);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, ServiceType type);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.RemoveService removeService,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, ServiceType type)
        {
            var serviceSchedule = await repository.Get(id);

            var command = new RemoveServiceCommand(Type: type);
            removeService.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
