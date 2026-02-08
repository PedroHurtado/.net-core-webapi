namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class SetPriceRange : IFeatureModule
{
    public record Request(
        decimal MinPrice,
        decimal MaxPrice);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer/price-range", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        await service.HandleAsync(request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Customer.SetPriceRange setPriceRange,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new SetPriceRangeCommand(
                MinPrice: request.MinPrice,
                MaxPrice: request.MaxPrice);

            setPriceRange.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
