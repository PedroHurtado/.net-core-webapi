namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class ConfigureServiceDay : IFeatureModule
{
    public record Request(
        bool IsAvailable,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        int? CapacityOverride);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/service-schedules/{id}/services/{type}/days/{day}", Handler)
            .WithDescriptionCatalog("Configure service day");
    }

    public static Func<IService, Guid, ServiceType, DayOfWeek, Request, Task<IResult>> Handler => async (service, id, type, day, request) =>
    {
        await service.HandleAsync(id, type, day, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, ServiceType type, DayOfWeek day, Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.ConfigureServiceDay configureServiceDay,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, ServiceType type, DayOfWeek day, Request request)
        {
            var serviceSchedule = await repository.Get(id);

            var command = new ConfigureServiceDayCommand(
                Type: type,
                Day: day,
                IsAvailable: request.IsAvailable,
                StartTime: request.StartTime,
                EndTime: request.EndTime,
                CapacityOverride: request.CapacityOverride);

            configureServiceDay.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
