namespace Customer.Features.Menus.Api.Queries.Allergens;

public class GetAllergen : IFeatureModule
{
    public record Response(
        string Id,
        string Name,
        string? IconUrl,
        bool IsActive,
        int DisplayOrder
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/allergens/{id}", async (IService service, string id) =>
        {
            var response = await service.HandleAsync(id);
            return Results.Ok(response);
        });
    }

    public interface IService
    {
        Task<Response> HandleAsync(string id);
    }

    [Injectable]
    public class Service(IRepository repository) : IService
    {
        public async Task<Response> HandleAsync(string id)
        {
            var allergen = await repository.Get(id);

            return new Response(
                allergen.Id,
                allergen.Name,
                allergen.IconUrl,
                allergen.IsActive,
                allergen.DisplayOrder
            );
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<Allergen, string> { }
}
