namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class AddService : IFeatureModule
{
    public record Request(
        ServiceType Type,
        int MaxCapacity,
        Dictionary<DayOfWeek, ServiceDayConfigInput> WeeklySchedule);

    public record ServiceDayConfigInput(
        bool IsAvailable,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        int? CapacityOverride);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/service-schedules/{id}/services", Handler)
            .WithDescriptionCatalog("Add service to schedule");
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        var response = await service.HandleAsync(id, request);
        return Results.Created($"/service-schedules/{response.Id}", response);
    };

    public interface IService
    {
        Task<ServiceScheduleResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.AddService addService,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ServiceScheduleResponse> HandleAsync(Guid id, Request request)
        {
            var serviceSchedule = await repository.Get(id);

            var weeklySchedule = request.WeeklySchedule.ToDictionary(
                kvp => kvp.Key,
                kvp => new CreateServiceDayConfigCommand(
                    kvp.Value.IsAvailable,
                    kvp.Value.StartTime,
                    kvp.Value.EndTime,
                    kvp.Value.CapacityOverride));

            var command = new AddServiceCommand(
                Type: request.Type,
                MaxCapacity: request.MaxCapacity,
                WeeklySchedule: weeklySchedule);

            addService.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();

            return ServiceScheduleResponse.Map(serviceSchedule);
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
