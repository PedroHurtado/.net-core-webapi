namespace Customers.Features.Customers.Api.CustomerAggregate.Commands;

public class RemoveImage : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/customer/images/{imageId}", Handler);
    }

    public static Func<IService, Guid, Task<IResult>> Handler => async (service, imageId) =>
    {
        await service.HandleAsync(imageId);
        return Results.NoContent();
    };

    public interface IService
    {
        Task HandleAsync(Guid imageId);
    }

    [Injectable]
    public class Service(
        Customer.RemoveImage removeImage,
        Guid tenantId,
        IRepository repository,
        IUnitOfWork unitOfWork) : IService
    {
        public async Task HandleAsync(Guid imageId)
        {
            var customer = await repository.Get(tenantId);

            var command = new RemoveImageCommand(ImageId: imageId);
            removeImage.Execute(customer, command);

            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IUpdate<Customer, Guid> { }
}
