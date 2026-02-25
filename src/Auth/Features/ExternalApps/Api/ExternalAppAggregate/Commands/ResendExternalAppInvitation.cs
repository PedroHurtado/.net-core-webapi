namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class ResendExternalAppInvitation : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.Ok();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/external-apps/{id}/resend-invitation", Handler)
            .WithDescriptionCatalog("Resend external app invitation");
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        ExternalApp.ResendInvitation resendInvitation,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);

            resendInvitation.Execute(entity);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<ExternalApp, Guid> { }
}
