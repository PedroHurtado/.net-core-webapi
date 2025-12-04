using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Fudie.DependencyInjection;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.pizzas.models;
using Fudie.Features;

namespace webapi.features.pizzas.queries;

public class GetPizzas : IFeatureModule
{
    public record Query(string? Name, int Page = 1, int Size = 25);

    public record IngredientResponse(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name
    );

    public record Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name,
        [Required][property: Required] string Description,
        [Required][property: Required] string Url,
        [Required][property: Required] decimal Price,
        [Required][property: Required] IEnumerable<IngredientResponse> Ingredients
    );

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/pizzas", async (IService service, IQuery repository, [AsParameters] Query query) =>
        {
            var queryResult = await service.Handler(query);
            return Results.Ok(queryResult);
        })
        .WithStandardOpenApi<List<Response>>(
            name: "GetPizzas",
            summary: "Recupera Pizzas",
            description: "Endpoint para recuperar pizzas paginadas con sus ingredientes",
            tag: "Pizzas",
            successStatusCode: StatusCodes.Status200OK
        );
    }

    public interface IService
    {
        Task<IQueryable<Response>> Handler(Query query);
    }

    [Injectable]
    public class Service(IQuery repository) : IService
    {
        private readonly IQuery _repository = repository;

        public Task<IQueryable<Response>> Handler(Query query)
        {
            var pizzasQuery = _repository.Query<Pizza>().Include(p => p.Ingredients);

            var result = pizzasQuery
                .Where(p => query.Name == null || p.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name)
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .Select(p => new Response(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Url,
                    p.Price,
                    p.Ingredients.Select(i => new IngredientResponse(i.Id, i.Name))
                ));

            return Task.FromResult(result);
        }
    }
}