namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class DeactivateExternalApp : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/external-apps/{id}/deactivate", Handler)
            .WithDescriptionCatalog("Deactivate external app");
    }

    public interface IService
    {
        Task<ExternalAppResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        ExternalApp.Deactivate deactivateExternalApp,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<ExternalAppResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);

            deactivateExternalApp.Execute(entity);

            await unitOfWork.SaveChangesAsync();

            return ExternalAppResponse.Map(entity);
        }
    }

    [Include<ExternalApp>("User")]
    public interface IRepository : IUpdate<ExternalApp, Guid> { }
}
