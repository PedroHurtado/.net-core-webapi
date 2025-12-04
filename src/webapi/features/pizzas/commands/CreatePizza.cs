using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Fudie.Domain;
using Fudie.DependencyInjection;
using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.ingredients.models;
using webapi.features.pizzas.models;
using Fudie.Features;

namespace webapi.features.pizzas.commands;

public class CreatePizza : IFeatureModule
{
    public record Request(
        [Required][property: Required] string Name,
        [Required][property: Required] string Description,
        [Required][property: Required] string Url,
        IEnumerable<Guid> Ingredients
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
        app.MapPost("/pizzas", async (IService service, Request request) =>
          {
              var response = await service.HandlerAsync(request);
              return Results.Created("", response);
          })
        .WithStandardOpenApi<Response>(
         name: "CreatePizza",
         summary: "Crear una nueva pizza",
         description: "Endpoint para crear una nueva pizza con su nombre, descripción, url e ingredientes",
         tag: "Pizzas",
         successStatusCode: StatusCodes.Status201Created,
         additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity, StatusCodes.Status404NotFound]
        );
    }

    public interface IService
    {
        Task<Response> HandlerAsync(Request request);
    }

    [Injectable]
    public class Service(
        IAdd<Pizza> pizzaRepository,
        IEntityLookup lookupRepository,
        IUnitOfWork unitOfWork
        ) : IService
    {
        private readonly IAdd<Pizza> pizzaRepository = pizzaRepository;
        private readonly IEntityLookup lookupRepository = lookupRepository;
        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Response> HandlerAsync(Request request)
        {
            var pizza = Pizza.Create(Guid.NewGuid(), request.Name, request.Description, request.Url).ValueOrThrow();

            foreach (var ingredientId in request.Ingredients)
            {
                var ingredient = await lookupRepository.GetRequiredAsync<Ingredient, Guid>(ingredientId);
                pizza.AddIngredient(ingredient).SuccessOrThrow();
            }

            pizzaRepository.Add(pizza);

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

    [Injectable]
    public class Repository(IChangeTracker repository) : IAdd<Pizza>
    {
        private readonly IChangeTracker _repository = repository;

        public void Add(Pizza entity)
        {
            _repository.Entry(entity).State = EntityState.Added;
        }
    }
}