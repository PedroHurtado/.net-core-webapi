namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class AcceptExternalAppInvitation : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/external-apps/{id}/accept", Handler)
            .WithDescriptionCatalog("Accept external app invitation");
    }

    public interface IService
    {
        Task<ApiKeyResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        CurrentUserId currentUserId,
        ExternalApp.AcceptInvitation acceptInvitation,
        IApiKeyGenerator apiKeyGenerator,
        IRepository repository,
        IEntityLookup entityLookup,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<ApiKeyResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);
            var user = await entityLookup.GetRequiredAsync<User, Guid>(currentUserId.Value);

            var apiKeyResult = apiKeyGenerator.Generate();

            var command = new AcceptExternalAppInvitationCommand(
                User: user,
                ApiKeyHash: apiKeyResult.Hash,
                ApiKeySalt: apiKeyResult.Salt,
                ApiKeyPrefix: apiKeyResult.Prefix);

            acceptInvitation.Execute(entity, command);

            await unitOfWork.SaveChangesAsync();

            return new ApiKeyResponse(
                ApiKey: apiKeyResult.RawKey,
                Prefix: apiKeyResult.Prefix,
                Message: "Store this key securely. It will not be shown again.");
        }
    }

    public interface IRepository : IUpdate<ExternalApp, Guid> { }
}
