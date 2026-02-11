namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class UpdateExternalApp : IFeatureModule
{
    public record Request(string Name);

    public static Func<IService, Guid, Request, Task<IResult>> Handler =>
        async (service, id, request) =>
        {
            await service.HandleAsync(id, request);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/external-apps/{id}", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        ExternalApp.Update updateExternalApp,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id, Request request)
        {
            var entity = await repository.Get(id);

            var command = new UpdateExternalAppCommand(
                Name: request.Name);

            updateExternalApp.Execute(entity, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ExternalApp, Guid> { }
}
