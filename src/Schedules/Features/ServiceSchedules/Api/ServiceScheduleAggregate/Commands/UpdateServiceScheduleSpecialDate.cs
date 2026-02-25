namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class UpdateServiceScheduleSpecialDate : IFeatureModule
{
    public record Request(
        bool IsAvailable,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        int? CapacityOverride,
        string? Reason);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/service-schedules/{id}/services/{type}/special-dates/{date}", Handler)
            .WithDescriptionCatalog("Update service special date");
    }

    public static Func<IService, Guid, ServiceType, DateOnly, Request, Task<IResult>> Handler => async (service, id, type, date, request) =>
    {
        await service.HandleAsync(id, type, date, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, ServiceType type, DateOnly date, Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.UpdateSpecialDate updateSpecialDate,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, ServiceType type, DateOnly date, Request request)
        {
            var serviceSchedule = await repository.Get(id);

            var command = new UpdateServiceScheduleSpecialDateCommand(
                Type: type,
                Date: date,
                IsAvailable: request.IsAvailable,
                StartTime: request.StartTime,
                EndTime: request.EndTime,
                CapacityOverride: request.CapacityOverride,
                Reason: request.Reason);

            updateSpecialDate.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
