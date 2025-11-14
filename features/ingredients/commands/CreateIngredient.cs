using webapi.common.infrastructure;
using webapi.features.ingredients.models;
using webapi.common;
using Microsoft.EntityFrameworkCore;

namespace webapi.features.ingredients.commands;

public class CreateIngredient
{

    public record Request(string Name, decimal Cost){}
    public record Response(Guid Id, string Name, decimal Cost) { }

    public interface IService
    {
        Task<Response> HandlerAsync(Request request);
    }

    public class Service(IAdd<Ingredient> respository, IUnitOfWork unitOfWork) : IService
    {
        private IAdd<Ingredient> _respository = respository;
        private IUnitOfWork _unifOfWork = unitOfWork;

        public async Task<Response> HandlerAsync(Request request)
        {
            var ingredient = Ingredient.Create(
                Guid.NewGuid(),
                request.Name,
                request.Cost
            ).ValueOrThrow();

            _respository.Add(ingredient);

            await _unifOfWork.SaveChangesAsync();

            return new Response(ingredient.Id, ingredient.Name, ingredient.Cost);
        }
    }

    public class Repository(IRespository repository) : IAdd<Ingredient>
    {
        private readonly IRespository _repository = repository;

        public void Add(Ingredient entity)
        {
            _repository.Entry(entity).State = EntityState.Added;
        }
    }
}