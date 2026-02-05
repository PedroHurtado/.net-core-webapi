namespace Menus.Features.Menus.Api.AllergenAggregate.Queries;

public class GetAllergens : IFeatureModule
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
        app.MapGet("/allergens", Handler);
    }

    public static Func<IService, Task<IResult>> Handler => async (service) =>
    {
        var response = await service.HandleAsync();
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<List<Response>> HandleAsync();
    }

    [Injectable]
    public class Service(IQuery repository) : IService
    {
        public async Task<List<Response>> HandleAsync()
        {
            var result = await repository.Query<Allergen>()
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .Select(a => new Response(
                    a.Id,
                    a.Name,
                    a.IconUrl,
                    a.IsActive,
                    a.DisplayOrder
                ))
                .ToListAsync();

            return result;
        }
    }
}
