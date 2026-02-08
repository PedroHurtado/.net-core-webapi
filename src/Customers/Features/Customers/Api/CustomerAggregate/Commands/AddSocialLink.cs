namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class AddSocialLink : IFeatureModule
{
    public record Request(
        string Platform,
        string Url);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/customer/social-links", Handler);
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
        Customer.AddSocialLink addSocialLink,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<CustomerResponse> HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new AddSocialLinkCommand(
                Platform: request.Platform,
                Url: request.Url);

            addSocialLink.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();

            return CustomerResponse.Map(customer);
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
