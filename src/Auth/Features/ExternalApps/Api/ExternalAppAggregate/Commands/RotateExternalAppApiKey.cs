namespace Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;

public class RotateExternalAppApiKey : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/external-apps/{id}/rotate-api-key", Handler);
    }

    public interface IService
    {
        Task<ApiKeyResponse> HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        ExternalApp.RotateApiKey rotateApiKey,
        IApiKeyGenerator apiKeyGenerator,
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task<ApiKeyResponse> HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);

            var apiKeyResult = apiKeyGenerator.Generate();

            var command = new RotateExternalAppApiKeyCommand(
                ApiKeyHash: apiKeyResult.Hash,
                ApiKeySalt: apiKeyResult.Salt,
                ApiKeyPrefix: apiKeyResult.Prefix);

            rotateApiKey.Execute(entity, command);

            await unitOfWork.SaveChangesAsync();

            return new ApiKeyResponse(
                ApiKey: apiKeyResult.RawKey,
                Prefix: apiKeyResult.Prefix,
                Message: "Store this key securely. It will not be shown again.");
        }
    }

    public interface IRepository : IUpdate<ExternalApp, Guid> { }
}
