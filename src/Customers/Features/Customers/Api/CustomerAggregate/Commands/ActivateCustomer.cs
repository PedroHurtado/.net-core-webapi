namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class ActivateCustomer : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/customer/activate", Handler)
            .WithDescriptionCatalog("Activate customer");
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
        Customer.Activate activateCustomer,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<CustomerResponse> HandleAsync()
        {
            var customer = await repository.Get(tenantId);

            activateCustomer.Execute(customer);

            await unitOfWork.SaveChangesAsync();

            return CustomerResponse.Map(customer);
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
