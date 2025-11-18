using System.ComponentModel.DataAnnotations;
using webapi.common;
using webapi.common.dependencyinjection;
using webapi.common.infrastructure;
using webapi.features.ingredients.models;

namespace webapi.features.ingredients.querys;

public class GetIngredients : IFeatureModule
{
    public record Query(string? Name, int Page = 1, int Size = 25);
    public record Response(
          [Required][property: Required] Guid Id,
          [Required][property: Required] string Name,
          [Required][property: Required] decimal Cost
      );
      
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/ingredients", async (IService service, IQuery repository, [AsParameters] Query query) =>
        {
            var queryResult = await service.Handler(query);            
            return Results.Ok(queryResult);
        })
        .WithStandardOpenApi<List<Response>>(
            name: "GetIngredients",
            summary: "Recupera Ingredients",
            description: "Endpoint para recuperar ingredientes paginados",
            tag: "Ingredientes",
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
            var ingredientsQuery = _repository.Query<Ingredient>();            

            var result = ingredientsQuery
                .Where(i => query.Name == null || i.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Name)
                .Skip((query.Page - 1) * query.Size)
                .Take(query.Size)
                .Select(i => new Response(
                    i.Id,
                    i.Name,
                    i.Cost
                ));

            return Task.FromResult(result);
        }
    }
}