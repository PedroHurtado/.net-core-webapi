namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class RemovePriceRange : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/customer/price-range", Handler);
    }

    public static Func<IService, Task<IResult>> Handler => async (service) =>
    {
        await service.HandleAsync();
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync();
    }

    [Injectable]
    public class Service(
        Customer.RemovePriceRange removePriceRange,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync()
        {
            var customer = await repository.Get(tenantId);

            removePriceRange.Execute(customer);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
