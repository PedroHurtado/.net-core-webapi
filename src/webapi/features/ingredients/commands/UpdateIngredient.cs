using Fudie.Infrastructure;
using Fudie.OpenApi;
using webapi.features.ingredients.models;
using Fudie.Domain;
using Microsoft.EntityFrameworkCore;
using Fudie.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Fudie.Features;



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
    public class Service(IRepositoy repository, IUnitOfWork unitOfWork /*,IRepository1 rep*/) : IService
    {

        private readonly IRepositoy _repository = repository;
        private readonly IUnitOfWork _unifOfWork = unitOfWork;
        //private readonly IRepository1 _rep = rep;
        public async Task HandlerAsync(Guid id, Request request)
        {            
            var ingredient = await _repository                
                .Get(id);
             //var other = await _rep.Get((id, id));

            ingredient.Update(request.Name, request.Cost).SuccessOrThrow();
            await _unifOfWork.SaveChangesAsync();
        }
    }

    //[Include<Category>("Categories", FilterBy = "Id")]
    //public interface IRepository1 : IGet<Menu, (Guid id, Guid categoryId)> {}


    public interface IRepositoy:IUpdate<Ingredient,Guid>{}
    /*[Injectable]
    public class Repository(IChangeTracker repository, IEntityLookup getOrThrowAsync) : IUpdate<Ingredient, Guid>
    {
        private readonly IChangeTracker _repository = repository;
        private readonly IEntityLookup _getOrThrowAsync = getOrThrowAsync;



        public Task<Ingredient> Get(Guid id)
        {
            return _getOrThrowAsync.GetRequiredAsync<Ingredient, Guid>(id);
        }


    }*/


}