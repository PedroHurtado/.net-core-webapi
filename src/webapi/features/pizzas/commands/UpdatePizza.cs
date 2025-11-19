using System.ComponentModel.DataAnnotations;
using webapi.common;
using webapi.common.dependencyinjection;
using webapi.common.infrastructure;

using webapi.features.pizzas.models;

namespace webapi.features.pizzas.commands;

public class UpdatePizza : IFeatureModule
{
    public record Request(
        [Required][property: Required] string Name,
        [Required][property: Required] string Description,
        [Required][property: Required] string Url
    );

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
        app.MapPut("/pizzas/{id:guid}", async (Guid id, IService service, Request request) =>
        {
            var response = await service.HandlerAsync(id, request);
            return Results.Ok(response);
        })
        .WithStandardOpenApi<Response>(
            name: "UpdatePizza",
            summary: "Actualizar una pizza existente",
            description: "Endpoint para actualizar el nombre, descripción y url de una pizza",
            tag: "Pizzas",
            successStatusCode: StatusCodes.Status200OK,
            additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity, StatusCodes.Status404NotFound]
        );
    }

    public interface IService
    {
        Task<Response> HandlerAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(
        IGetOrThrowAsync lookupRepository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        private readonly IGetOrThrowAsync lookupRepository = lookupRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Response> HandlerAsync(Guid id, Request request)
        {
            var pizza = await lookupRepository.GetOrThrowAsync<Pizza, Guid>(id);

            pizza.Update(request.Name, request.Description, request.Url).SuccessOrThrow();

            await unitOfWork.SaveChangesAsync();

            var response = new Response(
                pizza.Id,
                pizza.Name,
                pizza.Description,
                pizza.Url,
                pizza.Price,
                pizza.Ingredients.Select(i => new IngredientResponse(i.Id, i.Name))
            );

            return response;
        }
    }
}