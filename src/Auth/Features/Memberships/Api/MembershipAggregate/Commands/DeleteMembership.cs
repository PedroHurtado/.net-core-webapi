namespace Auth.Features.Memberships.Api.MembershipAggregate.Commands;

public class DeleteMembership : IFeatureModule
{
    public static Func<IService, Guid, Task<IResult>> Handler =>
        async (service, id) =>
        {
            await service.HandleAsync(id);
            return Results.NoContent();
        };

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/memberships/{id}", Handler)
            .WithDescriptionCatalog("Delete membership");
    }

    public interface IService
    {
        Task HandleAsync(Guid id);
    }

    [Injectable]
    public class Service(
        IRepository repository,
        IUnitOfWork unitOfWork
    ) : IService
    {
        public async Task HandleAsync(Guid id)
        {
            var entity = await repository.Get(id);
            repository.Remove(entity);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public interface IRepository : IRemove<Membership, Guid> { }
}
