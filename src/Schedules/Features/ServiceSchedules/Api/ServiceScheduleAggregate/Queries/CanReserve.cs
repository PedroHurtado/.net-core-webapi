namespace Schedules.Features.ServiceSchedules.Api.ServiceScheduleAggregate.Queries;

public class CanReserve : IFeatureModule
{
    public record Response(
        bool CanReserve,
        string? Reason);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/service-schedules/{id}/can-reserve", Handler)
            .WithDescriptionCatalog("Check if reservation is possible");
    }

    public static Func<IService, Guid, ServiceType, DateTime, int, Task<IResult>> Handler => async (service, id, type, dateTime, partySize) =>
    {
        var response = await service.HandleAsync(id, type, dateTime, partySize);
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<Response> HandleAsync(Guid id, ServiceType type, DateTime dateTime, int partySize);
    }

    [Injectable]
    public class Service(IRepository repository, TimeProvider timeProvider) : IService
    {
        public async Task<Response> HandleAsync(Guid id, ServiceType type, DateTime dateTime, int partySize)
        {
            var serviceSchedule = await repository.Get(id);

            var date = DateOnly.FromDateTime(dateTime);
            var time = TimeOnly.FromDateTime(dateTime);
            var now = timeProvider.GetUtcNow().DateTime;

            if (!serviceSchedule.IsServiceAvailableOn(type, date))
            {
                return new Response(false, "Service is not available on this date");
            }

            var config = serviceSchedule.GetServiceConfigForDate(type, date);
            if (config is null || !config.IsAvailable)
            {
                return new Response(false, "Service is not available on this date");
            }

            if (time < config.StartTime || time > config.EndTime)
            {
                return new Response(false, $"Service is only available from {config.StartTime} to {config.EndTime}");
            }

            var policy = serviceSchedule.Policy;

            if (!policy.IsWithinAdvanceWindow(dateTime, now))
            {
                var minHours = (int)policy.MinimumAdvanceTime.TotalHours;
                return new Response(false, $"Must book at least {minHours} hours in advance");
            }

            if (!policy.IsPartySizeValid(partySize))
            {
                return new Response(false, $"Maximum {policy.MaxPartySize} guests per reservation");
            }

            if (!policy.IsValidSlot(time))
            {
                var validSlots = policy.SlotIntervalMinutes switch
                {
                    15 => ":00, :15, :30, :45",
                    30 => ":00, :30",
                    60 => ":00",
                    _ => $"every {policy.SlotIntervalMinutes} minutes"
                };
                return new Response(false, $"Reservations only accepted at {validSlots}");
            }

            var duration = policy.GetDurationFor(type);
            var endTime = time.Add(duration);
            if (endTime > config.EndTime)
            {
                return new Response(false, "Not enough time for this reservation before service closes");
            }

            return new Response(true, null);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<ServiceSchedule, Guid> { }
}
