namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class AddSpecialDate : IFeatureModule
{
    public record Request(
        DateOnly Date,
        bool IsClosed,
        string Reason,
        SetTimeSlotRequest[] TimeSlots);

    public record SetTimeSlotRequest(
        TimeOnly OpenTime,
        TimeOnly CloseTime);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedules/{id}/special-dates", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, id, request) =>
    {
        var response = await service.HandleAsync(id, request);
        return Results.Created($"/schedules/{id}", response);
    };

    public interface IService
    {
        Task<ScheduleResponse> HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        Schedule.AddSpecialDate addSpecialDate,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ScheduleResponse> HandleAsync(Guid id, Request request)
        {
            var schedule = await repository.Get(id);

            var command = new AddSpecialDateCommand(
                Date: request.Date,
                IsClosed: request.IsClosed,
                Reason: request.Reason,
                TimeSlots: request.TimeSlots
                    .Select(ts => new CreateTimeSlotCommand(ts.OpenTime, ts.CloseTime))
                    .ToArray());

            addSpecialDate.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();

            return ScheduleResponse.Map(schedule);
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
