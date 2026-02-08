namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class AddImage : IFeatureModule
{
    public record Request(
        string Url,
        string? AltText,
        int DisplayOrder = 0,
        bool IsCover = false);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/customer/images", Handler);
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
        Customer.AddImage addImage,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task<CustomerResponse> HandleAsync(Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new AddImageCommand(
                Url: request.Url,
                AltText: request.AltText,
                DisplayOrder: request.DisplayOrder,
                IsCover: request.IsCover);

            addImage.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();

            return CustomerResponse.Map(customer);
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
