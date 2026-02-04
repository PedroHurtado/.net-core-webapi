namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class UpdateServiceSchedule : IFeatureModule
{
    public record Request(
        string Name,
        string Description,
        TimeSpan MinimumAdvanceTime,
        TimeSpan MaximumAdvanceTime,
        TimeSpan SlotInterval,
        TimeSpan BufferBetweenReservations,
        int MaxPartySize,
        int MinPartySize,
        Dictionary<ServiceType, TimeSpan> StandardDurations);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/service-schedules/{id}", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        await service.HandleAsync(id, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.Update updateServiceSchedule,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var serviceSchedule = await repository.Get(id);

            var command = new UpdateServiceScheduleCommand(
                Name: request.Name,
                Description: request.Description,
                MinimumAdvanceTime: request.MinimumAdvanceTime,
                MaximumAdvanceTime: request.MaximumAdvanceTime,
                SlotInterval: request.SlotInterval,
                BufferBetweenReservations: request.BufferBetweenReservations,
                MaxPartySize: request.MaxPartySize,
                MinPartySize: request.MinPartySize,
                StandardDurations: request.StandardDurations);

            updateServiceSchedule.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
