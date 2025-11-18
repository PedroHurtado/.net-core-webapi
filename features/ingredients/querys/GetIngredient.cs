
using System.ComponentModel.DataAnnotations;
using webapi.common;
using webapi.common.dependencyinjection;
using webapi.common.infrastructure;
using webapi.common.openapi;
using webapi.features.ingredients.models;

namespace webapi.features.ingredients.querys;

public class GetIngredient:IFeatureModule
{
    public record  Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name, 
        [Required][property: Required] decimal Cost
    );
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/ingredients/{id:guid}", async (Guid id, IGet<Ingredient,Guid> repository) =>
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
    class Repository(IGetOrThrowAsync repository) : IGet<Ingredient, Guid>
    {
        private readonly IGetOrThrowAsync _repository = repository;

        public Task<Ingredient> Get(Guid id)
        {
            return _repository.GetOrThrowAsync<Ingredient, Guid>(id, false);
        }
    }
}
