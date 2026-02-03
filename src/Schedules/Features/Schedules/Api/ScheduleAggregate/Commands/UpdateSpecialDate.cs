namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class UpdateSpecialDate : IFeatureModule
{
    public record Request(
        bool IsClosed,
        string Reason,
        SetTimeSlotRequest[] TimeSlots);

    public record SetTimeSlotRequest(
        TimeOnly OpenTime,
        TimeOnly CloseTime);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/schedules/{id}/special-dates/{date}", Handler);
    }

    public static Func<IService, Guid, DateOnly, Request, Task<IResult>> Handler => async (service, id, date, request) =>
    {
        await service.HandleAsync(id, date, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, DateOnly date, Request request);
    }

    [Injectable]
    public class Service(
        Schedule.UpdateSpecialDate updateSpecialDate,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, DateOnly date, Request request)
        {
            var schedule = await repository.Get(id);

            var command = new UpdateSpecialDateCommand(
                Date: date,
                IsClosed: request.IsClosed,
                Reason: request.Reason,
                TimeSlots: request.TimeSlots
                    .Select(ts => new CreateTimeSlotCommand(ts.OpenTime, ts.CloseTime))
                    .ToArray());

            updateSpecialDate.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
