namespace Auth.Features.Users.Api.UserAggregate.Queries;

public class GetMe : IFeatureModule
{
    public record MeTenantEntry(Guid TenantId);

    public record Response(
        Guid Id,
        string ProviderId,
        AuthProvider Provider,
        string Email,
        string Name,
        string? Phone,
        string? AvatarUrl,
        DateTime? LastLoginAt,
        bool IsActive,
        List<MeTenantEntry> Tenants);

    public static Func<IService, Task<IResult>> Handler =>
        async (service) =>
        {
            var response = await service.HandleAsync();
            return Results.Ok(response);
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/me", Handler)
            .RequireAuthenticated()
            .WithDescriptionCatalog("Get current user");
    }

    public interface IService
    {
        Task<Response> HandleAsync();
    }

    [Injectable]
    public class Service(
        CurrentUserId currentUserId,
        IMembershipLookup membershipLookup,
        IRepository repository) : IService
    {
        public async Task<Response> HandleAsync()
        {
            var user = await repository.Get(currentUserId.Value);

            var memberships = await membershipLookup.FindAllByUserId(user.Id);

            var tenants = memberships
                .Select(m => new MeTenantEntry(m.TenantId))
                .ToList();

            return new Response(
                user.Id,
                user.ProviderId,
                user.Provider,
                user.Email,
                user.Name,
                user.Phone,
                user.AvatarUrl,
                user.LastLoginAt,
                user.IsActive,
                tenants);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<User, Guid> { }
}
