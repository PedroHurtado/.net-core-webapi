namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class DeactivateCustomer : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/customer/deactivate", Handler)
            .WithDescriptionCatalog("Deactivate customer");
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
        Customer.Deactivate deactivateCustomer,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<CustomerResponse> HandleAsync()
        {
            var customer = await repository.Get(tenantId);

            deactivateCustomer.Execute(customer);

            await unitOfWork.SaveChangesAsync();

            return CustomerResponse.Map(customer);
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
