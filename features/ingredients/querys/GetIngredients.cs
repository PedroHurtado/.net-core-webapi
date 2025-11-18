using System.ComponentModel.DataAnnotations;
using webapi.common;
using webapi.common.infrastructure;
using webapi.features.ingredients.models;

namespace webapi.features.ingredients.querys;

public class GetIngredients : IFeatureModule
{
    public record Query(string? Name, int Page = 1, int Size = 25);
    public record Response(
          [Required][property: Required] Guid Id,
          [Required][property: Required] string Name,
          [Required][property: Required] decimal Cost
      );
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        throw new NotImplementedException();
    }
    public interface IService
    {
        Task<IEnumerable<Response>> Handler(Query query);
    }
    public class Service(IQuery repository) : IService
    {
        private readonly IQuery _repository = repository;
        public Task<IEnumerable<Response>> Handler(Query query)
        {
         
            throw new NotImplementedException();
        }
    }

}