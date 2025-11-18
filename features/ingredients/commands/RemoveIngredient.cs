using webapi.common.infrastructure;
using webapi.features.ingredients.models;
using webapi.common;
using Microsoft.EntityFrameworkCore;
using webapi.common.dependencyinjection;

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
        summary:"Elimina un ingrediente",
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
    public class Service(IRemove<Ingredient, Guid> repository, IUnitOfWork unitOfWork) : IService
    {

        private IRemove<Ingredient, Guid> _repository = repository;
        private IUnitOfWork _unifOfWork = unitOfWork;

        public async Task HandlerAsync(Guid id)
        {
            var ingredient = await _repository.Get(id);

            _repository.Remove(ingredient);

            await _unifOfWork.SaveChangesAsync();
        }
    }
    [Injectable]
    public class Repository(IRepository repository, IGetOrThrowAsync getOrThrowAsync) : IRemove<Ingredient, Guid>
    {
        private readonly IRepository _repository = repository;
        private readonly IGetOrThrowAsync _getOrThrowAsync = getOrThrowAsync;



        public Task<Ingredient> Get(Guid id)
        {
            return _getOrThrowAsync.GetOrThrowAsync<Ingredient, Guid>(id);
        }

        public void Remove(Ingredient entity)
        {
            _repository.Entry(entity).State = EntityState.Deleted;
        }


    }


}