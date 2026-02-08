namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class AddSupportedCulture : IFeatureModule
{
    public record Request(string Code);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/customer/supported-cultures", Handler);
    }

    public static Func<IService, Request, Task<IResult>> Handler => async (service, request) =>
    {
        var response = await service.HandleAsync(request);
        return Results.Created("/customer", response);
    };

    public interface IService
    {
        Task<CustomerResponse> HandleAsync(Request request);
    }

    [Injectable]
    public class Service(
        Customer.AddSupportedCulture addSupportedCulture,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<CustomerResponse> HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new AddSupportedCultureCommand(Code: request.Code);

            addSupportedCulture.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();

            return CustomerResponse.Map(customer);
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
