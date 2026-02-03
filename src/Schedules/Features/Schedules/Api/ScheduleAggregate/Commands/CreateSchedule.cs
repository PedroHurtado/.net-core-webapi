namespace Schedules.Features.Schedules.Api.ScheduleAggregate.Commands;

public class CreateSchedule : IFeatureModule
{
    public record Request(
        string Name,
        string? Description);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedules", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created($"/schedules/{response.Id}", response);
    };

    public interface IService
    {
        Task<ScheduleResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Schedule.Create createSchedule,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ScheduleResponse> HandleAsync(Request request)
        {
            var command = new CreateScheduleCommand(
                TenantId: tenantId,
                Name: request.Name,
                Description: request.Description);

            var schedule = createSchedule.Execute(command);

            repository.Add(schedule);
            await unitOfWork.SaveChangesAsync();

            return ScheduleResponse.Map(schedule);
        }
    }

    public interface IRepository : IAdd<Schedule> { }
}
