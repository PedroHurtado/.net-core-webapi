namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class RemoveServiceScheduleSpecialDate : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/service-schedules/{id}/services/{type}/special-dates/{date}", Handler);
    }

    public static Func<IService, Guid, ServiceType, DateOnly, Task<IResult>> Handler => async (service, id, type, date) =>
    {
        await service.HandleAsync(id, type, date);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid id, ServiceType type, DateOnly date);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.RemoveSpecialDate removeSpecialDate,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid id, ServiceType type, DateOnly date)
        {
            var serviceSchedule = await repository.Get(id);

            var command = new RemoveServiceScheduleSpecialDateCommand(
                Type: type,
                Date: date);

            removeSpecialDate.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
