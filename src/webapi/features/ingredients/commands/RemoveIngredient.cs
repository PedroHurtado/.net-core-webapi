using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.ingredients.models;
using Microsoft.EntityFrameworkCore;
using Fudie.DependencyInjection;
using Fudie.Features;

namespace webapi.features.ingredients.commands;

public class RemoveIngredient : IFeatureModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapDelete("/ingredientes/{id:guid}", async (IService service, Guid id) =>
        {
            await service.HandlerAsync(id);
            return Results.NoContent();
        })
        .WithStandardOpenApi(
         name: "RemoveIngreient",
         summary: "Elimina un ingrediente",
         description: "Endpoint para eliminar un ingrediente por id",
         tag: "Ingredientes",
         successStatusCode: StatusCodes.Status204NoContent,
         additionalErrorCodes: [StatusCodes.Status404NotFound]
        );

    }



    public interface IService
    {
        Task HandlerAsync(Guid id);
    }

    [Injectable]
    public class Service(IRepository repository, IUnitOfWork unitOfWork) : IService
    {

        private readonly IRepository _repository = repository;
        private readonly IUnitOfWork _unifOfWork = unitOfWork;

        public async Task HandlerAsync(Guid id)
        {
            var ingredient = await _repository.Get(id);

            _repository.Remove(ingredient);

            await _unifOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository:IRemove<Ingredient,Guid>{}

    /*[Injectable]
    public class Repository(IChangeTracker repository, IEntityLookup getOrThrowAsync) : IRemove<Ingredient, Guid>
    {
        private readonly IChangeTracker _repository = repository;
        private readonly IEntityLookup _getOrThrowAsync = getOrThrowAsync;



        public Task<Ingredient> Get(Guid id)
        {
            return _getOrThrowAsync.GetRequiredAsync<Ingredient, Guid>(id);
        }

        public void Remove(Ingredient entity)
        {
            _repository.Entry(entity).State = EntityState.Deleted;
        }


    }*/


}