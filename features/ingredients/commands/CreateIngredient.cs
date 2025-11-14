using webapi.common.infrastructure;
using webapi.features.ingredients.models;
using webapi.common;
using Microsoft.EntityFrameworkCore;
using webapi.common.dependencyinjection;

namespace webapi.features.ingredients.commands;

public class CreateIngredient
{

    public record Request(string Name, decimal Cost){}
    public record Response(Guid Id, string Name, decimal Cost) { }

    public interface IService
    {
        Task<Response> HandlerAsync(Request request);
    }

    [Injectable]
    public class Service(IAdd<Ingredient> repository, IUnitOfWork unitOfWork) : IService
    {
        private IAdd<Ingredient> _repository = repository;
        private IUnitOfWork _unifOfWork = unitOfWork;

        public async Task<Response> HandlerAsync(Request request)
        {
            var ingredient = Ingredient.Create(
                Guid.NewGuid(),
                request.Name,
                request.Cost
            ).ValueOrThrow();

            _repository.Add(ingredient);

            await _unifOfWork.SaveChangesAsync();

            return new Response(ingredient.Id, ingredient.Name, ingredient.Cost);
        }
    }

    //public interface IRespository:IAdd<Ingredient>{}
    [Injectable]
    public class Repository(IRepository repository) : IAdd<Ingredient>
    {
        private readonly IRepository _repository = repository;

        public void Add(Ingredient entity)
        {
            _repository.Entry(entity).State = EntityState.Added;
        }
    }
}