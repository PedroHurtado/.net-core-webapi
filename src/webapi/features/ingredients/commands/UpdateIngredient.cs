using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.ingredients.models;
using Fudie;
using Fudie.Domain;
using Microsoft.EntityFrameworkCore;
using Fudie.DependencyInjection;
using System.ComponentModel.DataAnnotations;



namespace webapi.features.ingredients.commands;

public class UpdateIngredient : IFeatureModule
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

        app.MapPut("/ingredientes/{id:guid}", async (IService service, Guid id, Request request) =>
       {
           await service.HandlerAsync(id, request);
           return Results.NoContent();
       })
       .WithStandardOpenApi(
        name: "UpateIngredient",
        summary: "Modificar un nuevo ingrediente",
        description: "Endpoint para modificar un ingrediente",
        tag: "Ingredientes",
        successStatusCode: StatusCodes.Status204NoContent,
        additionalErrorCodes: [StatusCodes.Status422UnprocessableEntity, StatusCodes.Status404NotFound]
       );

    }



    public interface IService
    {
        Task HandlerAsync(Guid id, Request request);
    }

    [Injectable]
    public class Service(IUpdate<Ingredient, Guid> repository, IUnitOfWork unitOfWork) : IService
    {

        private readonly IUpdate<Ingredient, Guid> _repository = repository;
        private readonly IUnitOfWork _unifOfWork = unitOfWork;

        public async Task HandlerAsync(Guid id, Request request)
        {
            var ingredient = await _repository.Get(id);
            ingredient.Update(request.Name, request.Cost).SuccessOrThrow();
            await _unifOfWork.SaveChangesAsync();
        }
    }

    //public interface IRespository:IAdd<Ingredient>{}
    [Injectable]
    public class Repository(IChangeTracker repository, IEntityLookup getOrThrowAsync) : IUpdate<Ingredient, Guid>
    {
        private readonly IChangeTracker _repository = repository;
        private readonly IEntityLookup _getOrThrowAsync = getOrThrowAsync;



        public Task<Ingredient> Get(Guid id)
        {
            return _getOrThrowAsync.GetRequiredAsync<Ingredient, Guid>(id);
        }


    }


}