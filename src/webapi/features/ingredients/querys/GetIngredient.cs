
using System.ComponentModel.DataAnnotations;
using Fudie.DependencyInjection;
using Fudie.Features;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.ingredients.models;

namespace webapi.features.ingredients.querys;

public class GetIngredient : IFeatureModule
{
    public record Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name,
        [Required][property: Required] decimal Cost
    );
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/ingredients/{id:guid}", async (Guid id, IGet<Ingredient, Guid> repository) =>
        {
            var ingredient = await repository.Get(id);
            var response = new Response(
                ingredient.Id,
                ingredient.Name,
                ingredient.Cost
            );
            return Results.Ok(response);
        })
        .WithOpenApi()
       .WithName("GetIngredient")
       .WithSummary("Recuperar un ingredient")
       .WithDescription("Endpoint para recuperar un ingrediente por id")
       .WithTags("Ingredientes")
       .Produces<Response>(StatusCodes.Status200OK)
       .Produces<CustomProblemDetails>(StatusCodes.Status404NotFound);
    }
    [Injectable]
    public class Repository(IEntityLookup repository) : IGet<Ingredient, Guid>
    {
        private readonly IEntityLookup _repository = repository;

        public Task<Ingredient> Get(Guid id)
        {
            return _repository.GetRequiredAsync<Ingredient, Guid>(id, false);
        }
    }
}
