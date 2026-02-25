namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class UpdateService : IFeatureModule
{
    public record Request(
        int MaxCapacity,
        Dictionary<DayOfWeek, ServiceDayConfigInput> WeeklySchedule);

    public record ServiceDayConfigInput(
        bool IsAvailable,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        int? CapacityOverride);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/service-schedules/{id}/services/{type}", Handler)
            .WithDescriptionCatalog("Update service");
    }

    public static Func<IService, Guid, ServiceType, Request, Task<IResult>> Handler => async (service, id, type, request) =>
    {
        await service.HandleAsync(id, type, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, ServiceType type, Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.UpdateService updateService,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, ServiceType type, Request request)
        {
            var serviceSchedule = await repository.Get(id);

            var weeklySchedule = request.WeeklySchedule.ToDictionary(
                kvp => kvp.Key,
                kvp => new CreateServiceDayConfigCommand(
                    kvp.Value.IsAvailable,
                    kvp.Value.StartTime,
                    kvp.Value.EndTime,
                    kvp.Value.CapacityOverride));

            var command = new UpdateServiceCommand(
                Type: type,
                MaxCapacity: request.MaxCapacity,
                WeeklySchedule: weeklySchedule);

            updateService.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
