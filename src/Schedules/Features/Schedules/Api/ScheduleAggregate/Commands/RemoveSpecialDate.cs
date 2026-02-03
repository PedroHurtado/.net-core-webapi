namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class RemoveSpecialDate : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/schedules/{id}/special-dates/{date}", Handler);
    }

    public static Func<IService, Guid, DateOnly, Task<IResult>> Handler => async (service, id, date) =>
    {
        await service.HandleAsync(id, date);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, DateOnly date);
    }

    [Injectable]
    public class Service(
        Schedule.RemoveSpecialDate removeSpecialDate,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, DateOnly date)
        {
            var schedule = await repository.Get(id);

            var command = new RemoveSpecialDateCommand(Date: date);
            removeSpecialDate.Execute(schedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Schedule, Guid> { }
}
