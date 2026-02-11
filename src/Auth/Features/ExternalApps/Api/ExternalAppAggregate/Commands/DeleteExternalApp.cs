namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class DeleteExternalApp : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/external-apps/{id}", Handler);
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);
            repository.Remove(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IRemove<ExternalApp, Guid> { }
}
