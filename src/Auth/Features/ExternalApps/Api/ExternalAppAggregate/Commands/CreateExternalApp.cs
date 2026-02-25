namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class CreateExternalApp : IFeatureModule
{
    public record Request(string Name, string InvitationEmail);

    public static Func<IService, Request, Task<IResult>> Handler =>
        async (service, request) =>
        {
            var response = await service.HandleAsync(request);
            return Results.Created($"/external-apps/{response.Id}", response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/external-apps", Handler)
            .WithDescriptionCatalog("Create external app");
    }

    public interface IService
    {
        Task<ExternalAppResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Guid tenantId,
        ExternalApp.Create createExternalApp,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<ExternalAppResponse> HandleAsync(Request request)
        {
            var duplicate = await repository.ExistsByInvitationEmail(request.InvitationEmail);
            ConflictGuard.ThrowIf(duplicate, "An external app with this email already exists in this tenant");

            var command = new CreateExternalAppCommand(
                TenantId: tenantId,
                Name: request.Name,
                InvitationEmail: request.InvitationEmail);

            var entity = createExternalApp.Execute(command);

            repository.Add(entity);
            await unitOfWork.SaveChangesAsync();

            return ExternalAppResponse.Map(entity);
        }
    }

    public interface IRepository : IAdd<ExternalApp>
    {
        [AsNoTracking]
        Task<bool> ExistsByInvitationEmail(string invitationEmail);
    }
}
