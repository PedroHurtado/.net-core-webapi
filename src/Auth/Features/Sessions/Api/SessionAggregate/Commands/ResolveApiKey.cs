namespace Auth.Features.Sessions.Api.SessionAggregate.Commands;

public class ResolveApiKey : IFeatureModule
{
    public static Func<IService, HttpContext, Task<IResult>> Handler =>
        async (service, httpContext) =>
        {
            var apiKey = httpContext.Request.Headers["X-Api-Key"].ToString();

            UnauthorizedGuard.ThrowIf(string.IsNullOrEmpty(apiKey), "API key is required");

            var token = await service.HandleAsync(apiKey);

            httpContext.Response.Headers["X-Auth-Token"] = token;

            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/resolve-api-key", Handler)
            .RequireInternal()
            .WithDescriptionCatalog("Resolve API key");
    }

    public interface IService
    {
        Task<string> HandleAsync(string apiKey);
    }

    [Injectable]
    public class Service(
        IInternalTokenService tokenService,
        IQuery query) : IService
    {
        public async Task<string> HandleAsync(string apiKey)
        {
            UnauthorizedGuard.ThrowIf(apiKey.Length < 8, "Invalid API key");

            var prefix = apiKey[..8];
            var externalApp = await query.Query<ExternalApp>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.ApiKeyPrefix == prefix)
                .FirstOrDefaultAsync();

            UnauthorizedGuard.ThrowIf(externalApp is null, "Invalid API key");

            var isValid = BCrypt.Net.BCrypt.Verify(apiKey, externalApp!.ApiKeyHash);
            UnauthorizedGuard.ThrowIf(!isValid, "Invalid API key");

            UnauthorizedGuard.ThrowIf(
                externalApp.InvitationStatus != InvitationStatus.Accepted,
                "External app invitation has not been accepted");

            UnauthorizedGuard.ThrowIf(
                !externalApp.IsActive,
                "External app is inactive");

            UnauthorizedGuard.ThrowIf(
                externalApp.User is null,
                "External app has no associated user");

            return tokenService.GenerateSessionToken(new SessionTokenData(
                externalApp.User!.Id,
                externalApp.TenantId,
                false,
                externalApp.Groups.ToArray(),
                externalApp.AdditionalScopes.ToArray(),
                externalApp.ExcludedScopes.ToArray()));
        }
    }

    [GenerateRepository<ExternalApp>]    
    [IgnoreQueryFilters]
    [AsNoTracking]
    [Include<ExternalApp>("User")]
    public interface IRepository
    {        
        Task<ExternalApp?> FindFirstByApiKeyPrefix(string apiKeyPrefix);
    }
}
