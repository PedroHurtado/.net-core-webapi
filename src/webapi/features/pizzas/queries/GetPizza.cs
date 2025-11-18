using System.ComponentModel.DataAnnotations;
using webapi.common;
using webapi.common.dependencyinjection;
using webapi.common.infrastructure;
using webapi.common.openapi;
using webapi.features.pizzas.models;

namespace webapi.features.pizzas.queries;

public class GetPizza : IFeatureModule
{
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
        app.MapGet("/pizzas/{id:guid}", async (Guid id, IGet<Pizza, Guid> repository) =>
        {
            var pizza = await repository.Get(id);
            var response = new Response(
                pizza.Id,
                pizza.Name,
                pizza.Description,
                pizza.Url,
                pizza.Price,
                pizza.Ingredients.Select(i => new IngredientResponse(i.Id, i.Name))
            );
            return Results.Ok(response);
        })
        .WithOpenApi()
        .WithName("GetPizza")
        .WithSummary("Recuperar una pizza")
        .WithDescription("Endpoint para recuperar una pizza por id con sus ingredientes")
        .WithTags("Pizzas")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces<CustomProblemDetails>(StatusCodes.Status404NotFound);
    }

    [Injectable]
    public class Repository(IGetOrThrowAsync repository) : IGet<Pizza, Guid>
    {
        private readonly IGetOrThrowAsync _repository = repository;

        public Task<Pizza> Get(Guid id)
        {
            return _repository.GetOrThrowAsync<Pizza, Guid>(
                id, 
                tracking:false,
                includeProperties:nameof(Pizza.Ingredients)
            
            );
        }
    }
}