namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Commands;

public class CreateServiceSchedule : IFeatureModule
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
        app.MapPost("/service-schedules", Handler)
            .WithDescriptionCatalog("Create a service schedule");
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created($"/service-schedules/{response.Id}", response);
    };

    public interface IService
    {
        Task<ServiceScheduleResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        ServiceSchedule.Create createServiceSchedule,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<ServiceScheduleResponse> HandleAsync(Request request)
        {
            var command = new CreateServiceScheduleCommand(
                TenantId: tenantId,
                Name: request.Name,
                Description: request.Description,
                MinimumAdvanceTime: request.MinimumAdvanceTime,
                MaximumAdvanceTime: request.MaximumAdvanceTime,
                SlotInterval: request.SlotInterval,
                BufferBetweenReservations: request.BufferBetweenReservations,
                MaxPartySize: request.MaxPartySize,
                MinPartySize: request.MinPartySize,
                StandardDurations: request.StandardDurations);

            var serviceSchedule = createServiceSchedule.Execute(command);

            repository.Add(serviceSchedule);
            await unitOfWork.SaveChangesAsync();

            return ServiceScheduleResponse.Map(serviceSchedule);
        }
    }

    public interface IRepository : IAdd<ServiceSchedule> { }
}
