namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class UpdateImage : IFeatureModule
{
    public record Request(
        string? AltText,
        int DisplayOrder,
        bool IsCover);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/customer/images/{imageId}", Handler);
    }

    public static Func<IService, Guid, Request, Task<IResult>> Handler => async (service, imageId, request) =>
    {
        await service.HandleAsync(imageId, request);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid imageId, Request request);
    }

    [Injectable]
    public class Service(
        Customer.UpdateImage updateImage,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid imageId, Request request)
        {
            var customer = await repository.Get(tenantId);

            var command = new UpdateImageCommand(
                ImageId: imageId,
                AltText: request.AltText,
                DisplayOrder: request.DisplayOrder,
                IsCover: request.IsCover);

            updateImage.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
