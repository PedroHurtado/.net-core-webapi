namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class ActivateServiceSchedule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/service-schedules/{id}/activate", Handler);
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, id) =>
    {
        var response = await service.HandleAsync(id);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<ServiceScheduleResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.Activate activateServiceSchedule,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ServiceScheduleResponse> HandleAsync(Guid id)
        {
            var serviceSchedule = await repository.Get(id);
            var currentActive = await repository.FindFirstByIsActiveTrue();

            var command = new ActivateServiceScheduleCommand(CurrentActive: currentActive);
            activateServiceSchedule.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();

            return ServiceScheduleResponse.Map(serviceSchedule);
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid>
    {
        [Tracking]
        Task<ServiceSchedule?> FindFirstByIsActiveTrue();
    }
}
