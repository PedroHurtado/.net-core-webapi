using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.ingredients.models;
using Fudie.Domain;
using Fudie.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Fudie.Features;
using webapi.domain.slice;

namespace webapi.features.ingredients.commands;

public class CreateIngredient : IFeatureModule
{
    public record Request(
        [Required][property: Required] string Name,
        [Required][property: Required] decimal Cost
    );
    public record Response(
        [Required][property: Required] Guid Id,
        [Required][property: Required] string Name,
        [Required][property: Required] decimal Cost
    );
    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPost("/ingredientes", async (IService service, Request request) =>
       {
           var response = await service.HandlerAsync(request);
           return Results.Created("", response);
       })
       .WithStandardOpenApi<Response>(
        name: "CreateIngredient",
        summary: "Crear un nuevo ingrediente",
        description: "Endpoint para crear un nuevo ingrediente con su nombre y costo",
        tag: "Ingredientes",
        successStatusCode: StatusCodes.Status201Created,
        additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity]
       );

    }



    public interface IService
    {
        Task<Response> HandlerAsync(Request request);
    }

    [Injectable]
    public class Service(IRepository repository,
    IUnitOfWork unitOfWork, ICreateFoo createFoo) : IService
    {

        /*private readonly IRepository _repo2=repo2;
        private readonly IAdd<Ingredient> _repository = repository;
        private readonly IUnitOfWork _unifOfWork = unitOfWork;*/

        public async Task<Response> HandlerAsync(Request request)
        {          
            createFoo.Handle();  
            
            var ingredient = Ingredient.Create(
                Guid.NewGuid(),
                request.Name,
                request.Cost
            ).ValueOrThrow();

            repository.Add(ingredient);


            await unitOfWork.SaveChangesAsync();

            return new Response(ingredient.Id, ingredient.Name, ingredient.Cost);
        }
    }

    public interface IRepository:IAdd<Ingredient>{}

    /*[Injectable]
    public class Repository(IChangeTracker repository) : IAdd<Ingredient>
    {
        private readonly IChangeTracker _repository = repository;

        public void Add(Ingredient entity)
        {
            _repository.Entry(entity).State = EntityState.Added;
        }
    }*/


}