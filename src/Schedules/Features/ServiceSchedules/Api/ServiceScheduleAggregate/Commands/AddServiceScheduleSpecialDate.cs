namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class AddServiceScheduleSpecialDate : IFeatureModule
{
    public record Request(
        DateOnly Date,
        bool IsAvailable,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        int? CapacityOverride,
        string? Reason);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/service-schedules/{id}/services/{type}/special-dates", Handler);
    }

    public static Func<IService, Guid, ServiceType, Request, Task<IResult>> Handler => async (service, id, type, request) =>
    {
        var response = await service.HandleAsync(id, type, request);
        return Results.Created($"/service-schedules/{response.Id}", response);
    };

    public interface IService
    {
        Task<ServiceScheduleResponse> HandleAsync(Guid id, ServiceType type, Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.AddSpecialDate addSpecialDate,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ServiceScheduleResponse> HandleAsync(Guid id, ServiceType type, Request request)
        {
            var serviceSchedule = await repository.Get(id);

            var command = new AddServiceScheduleSpecialDateCommand(
                Type: type,
                Date: request.Date,
                IsAvailable: request.IsAvailable,
                StartTime: request.StartTime,
                EndTime: request.EndTime,
                CapacityOverride: request.CapacityOverride,
                Reason: request.Reason);

            addSpecialDate.Execute(serviceSchedule, command);

            await unitOfWork.SaveChangesAsync();

            return ServiceScheduleResponse.Map(serviceSchedule);
        }
    }

    public interface IRepository : IUpdate<ServiceSchedule, Guid> { }
}
