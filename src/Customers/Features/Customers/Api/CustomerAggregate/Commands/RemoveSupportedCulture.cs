namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class RemoveSupportedCulture : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/customer/supported-cultures/{code}", Handler);
    }

    public static Func<IService, string, Task<IResult>> Handler => async (service, code) =>
    {
        await service.HandleAsync(code);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(string code);
    }

    [Injectable]
    public class Service(
        Customer.RemoveSupportedCulture removeSupportedCulture,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(string code)
        {
            var customer = await repository.Get(tenantId);

            var command = new RemoveSupportedCultureCommand(Code: code);
            removeSupportedCulture.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
