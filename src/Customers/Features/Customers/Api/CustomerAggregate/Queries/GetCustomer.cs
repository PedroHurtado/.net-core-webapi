namespace Customers.Features.Customers.Api.CustomerAggregate.Queries;

public class GetCustomer : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/customer", Handler)
            .WithDescriptionCatalog("Get current customer");
    }

    public static Func<IService, Task<IResult>> Handler => async (service) =>
    {
        var response = await service.HandleAsync();
        return Results.Ok(response);
    };

    public interface IService
    {
        Task<CustomerResponse> HandleAsync();
    }

    [Injectable]
    public class Service(
        Guid tenantId,
        IRepository repository) : IService
    {
        public async Task<CustomerResponse> HandleAsync()
        {
            var customer = await repository.Get(tenantId);
            return CustomerResponse.Map(customer);
        }
    }

    [AsNoTracking]
    public interface IRepository : IGet<Customer, Guid> { }
}
