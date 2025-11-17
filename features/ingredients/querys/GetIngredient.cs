/*
    IGet<Ingredient,Guid>
    class Repositori

    IGetOrThrowAsync
        GetOrThrowAsync<ingredient,Guid>(id,false)

    controlador->404

*/

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.OpenApi.Any;
using webapi.common;
using webapi.common.infrastructure;
using webapi.features.ingredients.models;

namespace webapi.features.ingredients.querys;

class GetIngredient:IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/ingredients", () =>
        {
            
        });
    }

    class Repository(IGetOrThrowAsync repository) : IGet<Ingredient, Guid>
    {
        private readonly IGetOrThrowAsync _repository = repository;

        public Task<Ingredient> Get(Guid id)
        {
            return _repository.GetOrThrowAsync<Ingredient, Guid>(id, false);
        }
    }
}
